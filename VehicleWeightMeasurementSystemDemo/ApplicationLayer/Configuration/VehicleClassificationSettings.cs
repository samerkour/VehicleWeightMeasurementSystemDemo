namespace VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration
{
    /// <summary>
    /// قوانین تعیین کلاس خودرو (CarClass13) و مجازبودن عبور.
    /// Defaults حدسی/پیش‌فرض هستند و باید با سند وب‌سرویس سامانه جامع هم‌راستا شوند.
    /// </summary>
    public class VehicleClassificationSettings
    {
        // 🔹 آستانه‌های وزن (kg) برای تشخیص کلاس در خودروهای ۲ محور
        public double SedanMaxWeight { get; set; } = 1500;    // سواری
        public double PickupMaxWeight { get; set; } = 3500;   // وانت
        public double LightTruckMaxWeight { get; set; } = 6000; // کامیون ۲ محور سبک

        // 🔹 آستانه وزن برای تفکیک کامیون/تریلر در خودروهای ۳ و ۴ محور
        public double Truck3MaxWeight { get; set; } = 18000;  // کامیون ۳ محور
        public double Truck4MaxWeight { get; set; } = 24000;  // کامیون ۴ محور

        // 🔹 جهت مورد انتظار عبور (DIR_COMING=1, DIR_DEPARTING=2)
        public byte ExpectedDirection { get; set; } = 1;

        // 🔹 حداقل دقت پلاک برای شمارنده‌شدن به‌عنوان خوانده‌شده
        public float MinConfidence { get; set; } = 0.65f;

        /// <summary>
        /// حداکثر وزن مجاز (kg) به تفکیک کلاس خودرو.
        /// اگر کلاس در این جدول نباشد، OverWeight صفر فرض می‌شود.
        /// </summary>
        public Dictionary<int, double> MaxAllowedWeightByClass { get; set; } = new()
        {
            [1] = 2200,   // سواری
            [2] = 3500,   // وانت
            [3] = 6000,   // کامیون ۲ محور سبک
            [4] = 10000,  // کامیون ۲ محور سنگین
            [5] = 15000,  // اتوبوس ۲ محور
            [6] = 23000,  // اتوبوس ۳ محور
            [7] = 18000,  // کامیون ۳ محور
            [8] = 23000,  // تریلر ۳ محور
            [9] = 24000,  // کامیون ۴ محور
            [10] = 27000, // تریلر ۴ محور ۱۲ چرخ
            [11] = 30000, // تریلر ۴ محور ۱۴ چرخ
            [12] = 32000, // تریلر ۵ محور ۱۲ چرخ
            [13] = 38000, // تریلر ۵ محور ۱۸ چرخ
            [14] = 40000  // تریلر ۶ محور
        };
    }
}