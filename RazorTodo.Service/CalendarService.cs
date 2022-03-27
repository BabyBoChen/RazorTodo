using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorTodo.DTO;
using RazorTodo.DAL;

namespace RazorTodo.Service
{
    public class CalendarService
    {
        public void ImportGovernmentCalendar(string filepath)
        {
            var gcds = this.ReadCsv(filepath);
            this.SaveGovernmentCalendar(gcds);
        }
    
        public List<GovernmentCalendarDate> ReadCsv(string filepath)
        {
            List<GovernmentCalendarDate> gcds = new List<GovernmentCalendarDate>();
            var readConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
            };
            try
            {
                using (var sr = new StreamReader(filepath))
                {
                    using (var csv = new CsvReader(sr, readConfiguration))
                    {
                        csv.Read();
                        while (csv.Read())
                        {
                            var gcd = new GovernmentCalendarDate();
                            var date = csv.GetField<string>(0);
                            var chineseDay = csv.GetField<string>(1);
                            var isHoliday = csv.GetField<int>(2);
                            var desc = csv.GetField<string>(3);
                            var day = this.ChineseDayToInt(chineseDay);
                            gcd.DateString = $"{date.Substring(0, 4)}-{date.Substring(4, 2)}-{date.Substring(6, 2)}";
                            gcd.Year = int.Parse(date.Substring(0, 4));
                            gcd.Month = int.Parse(date.Substring(4, 2));
                            gcd.Date = int.Parse(date.Substring(6, 2));
                            gcd.Day = day;
                            if (isHoliday == 0)
                            {
                                gcd.IsHoliday = false;
                            }
                            else
                            {
                                gcd.IsHoliday = true;
                            }
                            gcd.Description = desc;
                            gcds.Add(gcd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return gcds;
        }
        public int ChineseDayToInt(string chineseDay)
        {
            int d = 0;
            string cd = chineseDay.Trim();
            if (cd == "日")
            {
                d = 0;
            }
            else if (cd == "一")
            {
                d = 1;
            }
            else if (cd == "二")
            {
                d = 2;
            }
            else if (cd == "三")
            {
                d = 3;
            }
            else if (cd == "四")
            {
                d = 4;
            }
            else if(cd == "五")
            {
                d = 5;
            }
            else if(cd == "六")
            {
                d = 6;
            }
            return d;
        }
        public int SaveGovernmentCalendar(List<GovernmentCalendarDate> gcds)
        {
            int affectedRowCount = 0;
            using (var db = new RazorTodoContext())
            {
                foreach (var gcd in gcds)
                {
                    var gd = new GovernmentCalendar();
                    gd.DateString = gcd.DateString;
                    gd.Year = gcd.Year;
                    gd.Month = gcd.Month;
                    gd.Date = gcd.Date;
                    gd.Day = gcd.Day;
                    if (gcd.IsHoliday)
                    {
                        gd.IsHoliday = 1;
                    }
                    gd.Description = gcd.Description;
                    db.GovernmentCalendars.Add(gd);
                }
                affectedRowCount = db.SaveChanges();
            }
            return affectedRowCount;
        }
    }
}
