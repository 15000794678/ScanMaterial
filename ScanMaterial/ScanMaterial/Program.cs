using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phicomm_WMS.DB;
using System.Data;
using log4net;
using Phicomm_WMS.DataProcess;
using System.Diagnostics;
using SendMail_PHICOMM;

namespace ScanMaterial
{
    class Program
    {
        private static ILog Log = LogManager.GetLogger("ScanMaterial");
        private static int cntTotal = 0;
        private static int cntGuoQi = 0;
        private static int cntException = 0;

        static void Run()
        {
            try
            {
                Log.Debug(DateTime.Now.ToString() + " 启动运行物料过期扫描软件...");
                Trace.WriteLine(DateTime.Now.ToString() + " 启动运行物料过期扫描软件...");
                Console.WriteLine(DateTime.Now.ToString() + " 启动运行物料过期扫描软件...");

                //查询物料明细表
                Dictionary<string, object> dic = new Dictionary<string, object>();
                DataTable dt = new DataTable();
                string dateCode = string.Empty;
                string fifoDC = string.Empty;
                string message = string.Empty;
                int userdate = 0;
                int usedate_delay = 0;
                try
                {
                    dic.Add("status", "1"); //在库明细查询
                    SearchRInventoryDetail sr = new SearchRInventoryDetail(dic);
                    sr.ExecuteQuery();
                    dt = sr.GetResult();
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        message = "异常：SearchRInventoryDetail结果集为空!";
                        Console.WriteLine(message);
                        Trace.WriteLine(message);
                        Log.Error(message);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    message = "异常：SearchRInventoryDetail: " + ex.Message;
                    Console.WriteLine(message);
                    Trace.WriteLine(message);
                    Log.Error(message);
                    return;
                }
                                
                cntTotal = dt.Rows.Count;
                message = "共查询到：" + cntTotal + " 条记录！";
                Console.WriteLine(message);
                Trace.WriteLine(message);
                Log.Error(message);

                int i = 0;
                foreach (DataRow dr in dt.Rows)
                {//根据物料明细，查询基础表，获取使用期限，物料等级等信息
                    i++;
                    //if (i == 100)
                    //{
                    //    break;
                    //}
                    //物料唯一条码，料号，周期非空判断
                    if (string.IsNullOrEmpty(dr["TR_SN"].ToString().Trim()) ||
                        string.IsNullOrEmpty(dr["KP_NO"].ToString().Trim()) ||
                        string.IsNullOrEmpty(dr["DATE_CODE"].ToString().Trim())
                        )
                    {
                        cntException++;
                        message = "异常：NO=" + i + ", 信息不全，TR_SN=" + dr["TR_SN"].ToString().Trim() +
                                    ", KP_NO=" + dr["KP_NO"].ToString().Trim() +
                                    ", DATE_CODE=" + dr["DATE_CODE"].ToString().Trim();                        
                        Console.WriteLine(message);
                        Trace.WriteLine(message);
                        Log.Error(message);
                        continue;
                    }
                    dateCode = dr["DATE_CODE"].ToString().Trim();
                    fifoDC = DataCodeProcess.fifo_datecode(dateCode.ToUpper().Trim());

                    try
                    {
                        //查询使用期限，延长期限
                        dic.Clear();
                        dic.Add("material_no", dr["KP_NO"].ToString().Trim());
                        SearchBMaterial sb = new SearchBMaterial(dic);
                        sb.ExecuteQuery();
                        DataTable dt2 = sb.GetResult();
                        if (dt2 == null || dt.Rows.Count == 0)
                        {
                            cntException++;
                            message = "异常：NO=" + i + ", 查询使用期限失败， KP_NO=" + dr["KP_NO"].ToString().Trim();
                            Console.WriteLine(message);
                            Trace.WriteLine(message);
                            Log.Error(message);
                            continue;
                        }

                        if (string.IsNullOrEmpty(dt2.Rows[0]["material_level"].ToString().Trim()) ||
                            string.IsNullOrEmpty(dt2.Rows[0]["user_date"].ToString().Trim()))
                        {
                            cntException++;
                            message = "异常：NO=" + i + ", KP_NO=" + dr["KP_NO"].ToString().Trim() + "的物料等级或者使用期限为空， Level=" +
                                dt2.Rows[0]["material_level"].ToString().Trim() + ", USER_DATE=" + dt2.Rows[0]["user_date"].ToString().Trim();
                            Console.WriteLine(message);
                            Trace.WriteLine(message);
                            Log.Error(message);
                            continue;
                        }

                        //如果物料等级为2，需要更新算法
                        if (dt2.Rows[0]["material_level"].ToString().Trim().Equals("2"))
                        {
                            fifoDC = DataCodeProcess.GetFirstDayOfMonth(fifoDC);
                        }

                        //使用期限从字符串转为int型
                        userdate = 0;
                        if (!Int32.TryParse(dt2.Rows[0]["user_date"].ToString().Trim(), out userdate))
                        {
                            cntException++;
                            message = "异常：NO=" + i + ", 使用期限转化为int失败， KP_NO=" + dr["KP_NO"].ToString().Trim() + ", USER_DATE=" + dt2.Rows[0]["user_date"].ToString().Trim();
                            Console.WriteLine(message);
                            Trace.WriteLine(message);
                            Log.Error(message);
                            continue;
                        }
                        if (!string.IsNullOrEmpty(dt2.Rows[0]["usedate_delay"].ToString().Trim()))
                        {
                            if (!Int32.TryParse(dt2.Rows[0]["usedate_delay"].ToString().Trim(), out usedate_delay))
                            {                                 
                                cntException++;
                                message = "异常：NO=" + i + ", 延长期限转化为int失败， KP_NO=" + dr["KP_NO"].ToString().Trim() + ", USEDATE_DELAY=" + dt2.Rows[0]["usedate_delay"].ToString().Trim();
                                Console.WriteLine(message);
                                Trace.WriteLine(message);
                                Log.Error(message);
                                usedate_delay = 0; //continue;
                            }
                        }

                        //比较是否过期
                        int year = (userdate + usedate_delay) / 360;
                        int month = (userdate + usedate_delay) % 360 / 30;
                        int day = (userdate + usedate_delay) % 30;
                        int fifoDay = 0;
                        int CurrentDay = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
                        if (!Int32.TryParse(fifoDC, out fifoDay))
                        {
                            cntException++;
                            message = "异常：NO=" + i + ", fifoDC转换成Int失败： fifoDC=" + fifoDC;
                            Console.WriteLine(message);
                            Trace.WriteLine(message);
                            Log.Error(message);
                            continue;
                        }

                        fifoDay = fifoDay + year * 10000 + month * 100 + day;
                        message = "NO=" + i + ", TR_SN=" + dr["TR_SN"].ToString().Trim() +
                                              ", KP_NO=" + dr["KP_NO"].ToString().Trim() +
                                              ", DateCode=" + dateCode +
                                              ", fifoDC=" + fifoDC +
                                              ", UserDate=" + userdate.ToString() +
                                              ", UseDate_Delay=" + usedate_delay.ToString() + 
                                              ", 过期时间=" + fifoDay.ToString() +
                                              ", 当前时间=" + CurrentDay.ToString();
                        Trace.WriteLine(message);
                        if (fifoDay < CurrentDay)
                        {
                            cntGuoQi++;
                            Console.WriteLine(message);
                            Log.Error("物料过期：NO=" + i + ", " + message);

                            //将过期的物料状态从1更新为5         
                            try
                            {
                                UpdateRInventoryDetailStatus ur = new UpdateRInventoryDetailStatus(dr["TR_SN"].ToString().Trim(), "1", "5");
                                ur.ExecuteUpdate();
                            }
                            catch(Exception ex)
                            {
                                cntException++;
                                message = "UpdateRInventoryDetailStatus: " + ex.Message;
                                Console.WriteLine("异常：" + ex.Message);
                                Trace.WriteLine("异常：" + ex.Message);
                                Log.Error("异常：" + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("异常：" + ex.Message);
                        Trace.WriteLine("异常：" + ex.Message);
                        Log.Error("异常：" + ex.Message);
                        continue;
                    }
                }                
            }
            catch(Exception ex)
            {
                Console.WriteLine("异常：" + ex.Message);
                Trace.WriteLine("异常：" + ex.Message);
                Log.Error("异常：" + ex.Message);
            }
            finally
            {
                Console.WriteLine(DateTime.Now.ToString() + " 结束运行物料过期扫描软件!");
                Trace.WriteLine(DateTime.Now.ToString() + " 结束运行物料过期扫描软件!");
                Log.Debug(DateTime.Now.ToString() + " 结束运行物料过期扫描软件!");
            }
        }

        static void SendEMail()
        {
            try
            {
                MyEmail ee = new MyEmail("mail.phicomm.com", "huijuan.wan@phicomm.com", "", "", "huijuan.wan@phicom.com",
                                        "今日物料过期扫描结果：" + cntTotal + " 条记录， " + cntGuoQi + " 条过期， " + cntException + " 条异常！", "详细扫描结果请于附件查收!", "huijaun.wan", "", "25", true, false, true);
                //ee.AddAttachments(@"log4net.dll;");
                ee.Send();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            DateTime start_dt = DateTime.Now;
            DateTime end_st;

            Run();
            //if (cntGuoQi > 0)
            {
                SendEMail();
            }            

            end_st = DateTime.Now;
            int ts = (int)((end_st - start_dt).TotalSeconds);

            string str = "共查询到 ： " + cntTotal + " 条记录，其中：异常 " + cntException + " 条， 过期 " + cntGuoQi + " 条!";
            Console.WriteLine(str);
            Trace.WriteLine(str);
            Log.Debug(str);

            str = "共耗时：" + ts / 3600 + " 时 " + ts % 3600 / 60 + " 分 " + ts % 60 + " 秒";
            Console.WriteLine(str);
            Trace.WriteLine(str);
            Log.Debug(str);            
        }

        
    }
}
