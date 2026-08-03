using System;
using System.Globalization;

namespace VehicleWeightMeasurementSystemDemo.Controls
{
    /// <summary>
    /// Real Jalali (Shamsi) calendar helpers built on System.Globalization.PersianCalendar.
    /// No string hacks — all conversions go through PersianCalendar.ToDateTime / Get* methods.
    /// </summary>
    public static class PersianCalendarHelper
    {
        private static readonly PersianCalendar Persian = new();

        // هفته از شنبه شروع می‌شود
        public static readonly string[] DayNames =
        {
            "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه"
        };

        public static readonly string[] ShortDayNames =
        {
            "ش", "ی", "د", "س", "چ", "پ", "ج"
        };

        // ماه‌های شمسی (index 1..12)
        public static readonly string[] MonthNames =
        {
            "", "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";
        private const string EnglishDigits = "0123456789";

        /// <summary>تبدیل ارقام انگلیسی به فارسی</summary>
        public static string ToPersianDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int idx = EnglishDigits.IndexOf(chars[i]);
                if (idx >= 0)
                    chars[i] = PersianDigits[idx];
            }
            return new string(chars);
        }

        /// <summary>تبدیل ارقام فارسی به انگلیسی (برای Parse)</summary>
        public static string ToEnglishDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int idx = PersianDigits.IndexOf(chars[i]);
                if (idx >= 0)
                    chars[i] = EnglishDigits[idx];
            }
            return new string(chars);
        }

        /// <summary>
        /// قالب‌بندی یک تاریخ میلادی به صورت شمسی: yyyy/MM/dd (و در صورت نیاز ساعت)
        /// </summary>
        public static string FormatJalali(DateTime value, bool includeTime)
        {
            if (value == DateTime.MinValue || value == DateTime.MaxValue)
                return "";

            int year = Persian.GetYear(value);
            int month = Persian.GetMonth(value);
            int day = Persian.GetDayOfMonth(value);

            string date = ToPersianDigits($"{year:0000}/{month:00}/{day:00}");

            if (includeTime)
                date += " " + ToPersianDigits($"{value.Hour:00}:{value.Minute:00}");

            return date;
        }

        /// <summary>
        /// تبدیل یک رشته‌ی شمسی (با ارقام فارسی یا انگلیسی) به DateTime میلادی.
        /// فرمت‌های پشتیبانی: yyyy/MM/dd ، yyyy-MM-dd و نسخه‌های همراه با ساعت HH:mm
        /// </summary>
        public static bool TryParseJalali(string text, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string clean = ToEnglishDigits(text.Trim()).Replace('−', '-');

            string[] parts = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return false;

            string datePart = parts[0];
            string timePart = parts.Length > 1 ? parts[1] : "";

            int hour = 0, minute = 0;
            if (!string.IsNullOrWhiteSpace(timePart))
            {
                string[] tp = timePart.Split(':');
                if (tp.Length < 2 ||
                    !int.TryParse(tp[0], out hour) ||
                    !int.TryParse(tp[1], out minute) ||
                    hour < 0 || hour > 23 || minute < 0 || minute > 59)
                    return false;
            }

            string[] dp = datePart.Split(new[] { '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (dp.Length != 3)
                return false;

            if (!int.TryParse(dp[0], out int year) ||
                !int.TryParse(dp[1], out int month) ||
                !int.TryParse(dp[2], out int day))
                return false;

            if (month < 1 || month > 12 || day < 1 || day > 31)
                return false;

            try
            {
                // PersianCalendar به‌صورت بومی تاریخ شمسی را اعتبارسنجی می‌کند
                result = Persian.ToDateTime(year, month, day, hour, minute, 0, 0);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>مشخص کردن روز نخست ماه (0 = شنبه)</summary>
        public static int GetFirstDayOffset(int year, int month)
        {
            var first = Persian.ToDateTime(year, month, 1, 0, 0, 0, 0);
            // DayOfWeek در دات‌نت از یکشنبه (0) شروع می‌شود؛ ما از شنبه (0) شروع می‌کنیم
            return (((int)first.DayOfWeek) + 1) % 7;
        }

        public static int GetDaysInMonth(int year, int month)
            => Persian.GetDaysInMonth(year, month);

        public static int GetYear(DateTime value) => Persian.GetYear(value);
        public static int GetMonth(DateTime value) => Persian.GetMonth(value);
        public static int GetDay(DateTime value) => Persian.GetDayOfMonth(value);

        /// <summary>تاریخ میلادی نمایشی برای Tooltip</summary>
        public static string FormatGregorian(DateTime value)
            => value == DateTime.MinValue || value == DateTime.MaxValue
                ? ""
                : $"{value:yyyy/MM/dd HH:mm}";

        /// <summary>
        /// تبدیل اجزای تاریخ شمسی به DateTime میلادی (برای سلول‌های تقویم)
        /// </summary>
        public static DateTime ToGregorian(int year, int month, int day, int hour, int minute)
            => Persian.ToDateTime(year, month, day, hour, minute, 0, 0);
    }
}
