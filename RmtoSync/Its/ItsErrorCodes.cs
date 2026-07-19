namespace RmtoSync.Its;

/// <summary>Error / validation codes from ITS guide chapters 3-2 … 3-6.</summary>
public static class ItsErrorCodes
{
    public static class Login
    {
        public const long InvalidCredentials = 1000;
    }

    public static class AddTto
    {
        public const long OkNationalPlate = 0;
        public const long OkDamagedPlate = 1;
        public const long OkTransitPlate = 2;
        public const long OkAwaitingImage = 99;
        public const long Duplicate = 101;
        public const long InvalidDevice = 102;
        public const long DeviceCompanyMismatch = 103;
        public const long InvalidSystemCode = 104;
        public const long InvalidCompanyCode = 105;
        public const long InvalidReceiveDateTime = 106;
        public const long InvalidAllowedFlag = 107;
        public const long InvalidPlateLength = 108;
        public const long InvalidPlateLetter = 109;
        public const long ColorImageTooLarge = 110;
        public const long PlateImageTooLarge = 111;
        public const long InvalidCarClass = 112;
        public const long DataExpired = 113;
        public const long ZeroInPlateNotAllowed = 114;
        public const long HeavyOrViolationMissingImage = 115;
        public const long InvalidSpeedType = 116;
        public const long MissingCrimeCode = 117;
        public const long PassAfterReceive = 118;
        public const long PermittedWithCrime = 119;
        public const long ImageTooSmall = 120;
        public const long PassInFuture = 121;
        public const long ReceiveInFuture = 122;
        public const long InvalidPoliceCode = 123;
        public const long AverageSpeedError = 124;
        public const long DamagedPlateMissingImage = 125;
        public const long InvalidCarClassForWim = 126;
        public const long InvalidTransitPlateChars = 127;
        public const long UserNotAuthorized = 128;
        public const long ViolationNotAllowedForCamera = 129;
        public const long NullReferenceNo = 191;
        public const long MethodNotAllowed = 1011;
        public const long TokenExpired = 1002;
    }

    public static class Batch
    {
        public const long NullPayload = 1007;
        public const long TooManyRecords = 1008;
        public const long MethodNotAllowed = 1011;
    }

    public static class AddImage
    {
        public const long InvalidCredentials = 1000;
        public const long UserBlocked = 1001;
        public const long CredentialsExpired = 1002;
        public const long InvalidIp = 1003;
        public const long PlateImageTooLarge = 111;
        public const long SendWindowExpired = 1009;
        public const long NullPlateImage = 1012;
        public const long InvalidReference = 1013;
        public const long PlateAlreadySent = 1014;
        public const long NullReferenceNo = 1015;
        public const long ViolationOnly = 1016;
        public const long HeavyVehicleOnly = 1017;
        public const long NullDeviceId = 1018;
        public const long InvalidDeviceId = 1019;
        public const long MethodNotAllowed = 1011;
        public const long NormalTrafficWithoutPlate = 131;
    }

    private static readonly IReadOnlyDictionary<long, string> Descriptions = new Dictionary<long, string>
    {
        [Login.InvalidCredentials] = "نام کاربری یا کلمه عبور نادرست",
        [AddTto.OkNationalPlate] = "دریافت موفق با پلاک ملی",
        [AddTto.OkDamagedPlate] = "دریافت موفق با پلاک مخدوش",
        [AddTto.OkTransitPlate] = "دریافت موفق با پلاک ترانزیت",
        [AddTto.OkAwaitingImage] = "ارسال بدون عکس — مهلت 24 ساعت برای ارسال مجدد",
        [AddTto.Duplicate] = "تکراری بودن رکورد تردد",
        [AddTto.InvalidDevice] = "عدم صحت شناسه دستگاه",
        [AddTto.DeviceCompanyMismatch] = "عدم تطابق کد دستگاه با کد شرکت و سیستم",
        [AddTto.InvalidSystemCode] = "عدم صحت مقدار فیلد کد سیستم",
        [AddTto.InvalidCompanyCode] = "عدم صحت مقدار فیلد کد شرکت",
        [AddTto.InvalidReceiveDateTime] = "عدم صحت تاریخ تردد یا تاریخ ثبت در سرور فرستنده",
        [AddTto.InvalidAllowedFlag] = "عدم صحت مقدار فیلد مجاز/غیرمجاز",
        [AddTto.InvalidPlateLength] = "طول شماره پلاک صحیح نیست",
        [AddTto.InvalidPlateLetter] = "بخش حرفی پلاک اشتباه است",
        [AddTto.ColorImageTooLarge] = "اندازه تصویر اصلی بیش از 300 کیلوبایت",
        [AddTto.PlateImageTooLarge] = "اندازه تصویر پلاک بیش از 50 کیلوبایت",
        [AddTto.InvalidCarClass] = "عدم صحت کلاس خودرو",
        [AddTto.DataExpired] = "زمان مجاز برای ارسال اطلاعات منقضی شده (بیش از 30 روز)",
        [AddTto.ZeroInPlateNotAllowed] = "مقدار صفر در شماره پلاک مجاز نیست",
        [AddTto.HeavyOrViolationMissingImage] = "تردد سنگین یا متخلف فاقد تصویر پلاک یا خودرو",
        [AddTto.InvalidSpeedType] = "کد نوع سرعت اشتباه است",
        [AddTto.MissingCrimeCode] = "کد جریمه مشخص نشده (برای ترددهای متخلف)",
        [AddTto.PassAfterReceive] = "تاریخ تردد بزرگتر از تاریخ ثبت در سامانه استانی",
        [AddTto.PermittedWithCrime] = "تردد مجاز نمی‌تواند کد جریمه داشته باشد",
        [AddTto.ImageTooSmall] = "اندازه تصویر کوچکتر از حد مجاز",
        [AddTto.PassInFuture] = "تاریخ تردد بزرگتر از زمان جاری",
        [AddTto.ReceiveInFuture] = "تاریخ ثبت بزرگتر از زمان جاری",
        [AddTto.InvalidPoliceCode] = "کد پلیس نادرست",
        [AddTto.AverageSpeedError] = "خطا در ارسال سرعت میانگین",
        [AddTto.DamagedPlateMissingImage] = "تردد با پلاک مخدوش فاقد تصویر",
        [AddTto.InvalidCarClassForWim] = "مقدار CarClass13/CarClass15 برای WIM اشتباه",
        [AddTto.InvalidTransitPlateChars] = "پلاک ترانزیت دارای کاراکتر غیرمجاز",
        [AddTto.UserNotAuthorized] = "کاربر اجازه ارسال این تردد را ندارد",
        [AddTto.ViolationNotAllowedForCamera] = "تخلف برای این دوربین مجاز نیست",
        [AddTto.NullReferenceNo] = "مقدار ReferenceNo برابر Null",
        [AddTto.MethodNotAllowed] = "متد برای این کاربر غیرمجاز",
        [AddTto.TokenExpired] = "توکن منقضی شده",
        [Batch.NullPayload] = "مقدار گروه داده ارسالی برابر Null",
        [Batch.TooManyRecords] = "تعداد ترددهای ارسالی بیش از 100",
        [Batch.MethodNotAllowed] = "متد برای این کاربر غیرمجاز",
        [AddImage.InvalidCredentials] = "نام کاربری یا کلمه عبور نادرست",
        [AddImage.UserBlocked] = "کاربر مسدود است",
        [AddImage.CredentialsExpired] = "اطلاعات کاربری منقضی شده",
        [AddImage.InvalidIp] = "IP برای این کاربر نامعتبر",
        [AddImage.PlateImageTooLarge] = "اندازه تصویر پلاک بیش از 50 کیلوبایت",
        [AddImage.SendWindowExpired] = "مهلت ارسال تصویر (24 ساعت) منقضی شده",
        [AddImage.NullPlateImage] = "تصویر پلاک null",
        [AddImage.InvalidReference] = "شناسه ارجاع نامعتبر",
        [AddImage.PlateAlreadySent] = "تصویر پلاک قبلاً ارسال شده",
        [AddImage.NullReferenceNo] = "ReferenceNo برابر Null یا صفر",
        [AddImage.ViolationOnly] = "تصویر فقط برای تردد متخلف",
        [AddImage.HeavyVehicleOnly] = "تصویر مربوط به خودرو سنگین",
        [AddImage.NullDeviceId] = "DeviceId برابر null یا صفر",
        [AddImage.InvalidDeviceId] = "شناسه دستگاه اشتباه",
        [AddImage.NormalTrafficWithoutPlate] = "تردد عادی فاقد تصویر پلاک"
    };

    public static bool IsTtoSuccess(long code) =>
        code is AddTto.OkNationalPlate or AddTto.OkDamagedPlate or AddTto.OkTransitPlate
            or AddTto.OkAwaitingImage or AddTto.Duplicate;

    public static bool IsAddImageSuccess(long errorCode, long sysError) =>
        sysError == 0 && errorCode is 0 or AddTto.Duplicate or AddImage.PlateAlreadySent;

    public static string Describe(long code) =>
        Descriptions.TryGetValue(code, out var text) ? text : $"کد خطا {code}";
}
