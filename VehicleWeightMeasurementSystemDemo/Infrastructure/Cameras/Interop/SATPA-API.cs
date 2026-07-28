using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop
{


    enum ESAVE_PLATE_OPTION { SAVE_NOTHING, SAVE_PLATE_ONLY, SAVE_PLATE_AND_CAR };


    public class SLPRPropertyGrid
    {
        [Category("ا) پارامترهای پرکاربرد"), DisplayName("نرخ فریم بر ثانیه"), DefaultValue((byte)15), Description("تعداد فریم بر ثانیه دریافتی از دوربین. برای کسب نتیجه ایده آل، نرخ فریم خود دوربین را هم روی همین مقدار تنظیم کنید")]
        public byte frame_rate { get; set; } = 15;

        [Category("ا) پارامترهای پرکاربرد"), DisplayName("تعداد نویسه های پلاک"), Description("اگر فقط پلاک استاندارد مد نظر است، 8 و 0 بگذارید. اگر پلاک مناطق آزاد را هم می خواهید، 8 و 5 بگذارید.")]
        public int[] num_valid_chars { get; set; } = new int[2] { 8, 0 };  // Number of valid characters usuallay {8, 0}. if e.g. 5 character plates are also available, use {8, 5}

        [Category("ب) پارامترهای ویدیو"), DisplayName("گزارش پلاکهای ناقص"), DefaultValue(false), Description("اگر پلاکی کمتر از تعداد استاندارد، مثلا 8، رقم داشت یا تعداد حروف آن بیش از یکی باشد، ناقص تلقی می شود. با تیک زدن این گزینه، اینها هم گزارش خواهند شد.")]
        public bool report_non_standard_plates { get; set; } = false;

        [Category("ب) پارامترهای ویدیو"), DisplayName("VLC Cache Time"), DefaultValue((short)1000), Description("مدت زمان بافر کردن جریان شبکه برحسب میلی ثانیه، توسط کتابخانه وی ال سی. اگر ارتباط با دوربین برقرار نمی شود، این عدد را افزایش دهید")]
        public short vlc_net_cache_time { get; set; } = 1000;

        [Category("ب) پارامترهای ویدیو"), DisplayName("آستانه تشخیص خودرو بدون پلاک"), DefaultValue((short)1000), Description(" این تعداد فریم قبل را برای بررسی تشخیص دارا بودن خودرو بررسی کن. عدد -1 به معنی غیر فعال بودن است")]
        public int missed_car_threshold { get; set; } = -1;
        [Category("ب) پارامترهای ویدیو"), DisplayName("ضریب طولی  ناحیه خودرو بدون پلاک"), DefaultValue((float)0.02), Description(" خودرو هایی که مختصات طولی آن ها تا حاشیه چپ و راست تصویر کمتر از حاصل ضرب این عدد در طول تصویر باشند، مورد بررسی بدون پلاک قرار نی گیرند (غالبا خودرو های ناقص اند)")]
        public float x_coefficient_missed_car_area { get; set; } = (float)0.02;
        [Category("ب) پارامترهای ویدیو"), DisplayName("ضریب عرضی  ناحیه خودرو بدون پلاک"), DefaultValue((float)0.025), Description(" خودرو هایی که مختصات عرضی آن ها تا حاشیه بالا و پایین تصویر کمتر از حاصل ضرب این عدد در عرض تصویر باشند، مورد بررسی بدون پلاک قرار نی گیرند (غالبا خودرو های ناقص اند)")]
        public float y_coefficient_missed_car_area { get; set; } = (float)0.025;

        [Category("ب) پارامترهای ویدیو"), DisplayName("Gstreamer"), DefaultValue((short)1000), Description("استفاده از متد gstreamer برای اتصال به دوربین")]
        public byte gstreamer { get; set; } = 1;
        [Category("ب) پارامترهای ویدیو"), DisplayName("Tcp"), DefaultValue((short)1000), Description("برای اتصال با پروتکل tcp از این گزینه استفاده کنید")]
        public bool tcp { get; set; } = true;

        [Category("ب) پارامترهای ویدیو"), DisplayName("دریافت تک تصویر"), DefaultValue(false), Description("اگر در حالت عادی نمی توانید به دوربین وصل شوید، این گزینه را تیک بزنید و دوباره تلاش کنید")]
        public bool take_shots_from_camera { get; set; } = false;

        [Category("ج) پارامترهای عمومی"), DisplayName(" حداقل دقت پلاک زمان گزارش"), DefaultValue(false), Description("پلاک هایی با دقت کمتر نادیده گرفته خواهند شد")]
        public float min_cnf { get; set; } = 0.65f;
        //ب) پارامترهای ویدیو

        [Category("ب) پارامترهای ویدیو"), DisplayName("وقفه پس از موفقیت"), DefaultValue((byte)0), Description("بعد از ثبت موفق یک پلاک، این تعداد فریم را پردازش نکن")]
        public byte n_frm_skip_on_success { get; set; } = 0; //Number of frames to be skipped after successful plate detection

        [Category("ب) پارامترهای ویدیو"), DisplayName("عدم گزارش پلاک تکراری"), DefaultValue((byte)1), Description("برای جلوگیری از گزارش پلاک تکراری مقداری بر حسب تعداد فریم تنظیم نمایید")]
        public int skip_same_plate_frm { get; set; } = 50;

        [Category("ب) پارامترهای ویدیو"), DisplayName("اختلاف نویسه های پلاک تکراری"), DefaultValue((byte)1), Description(".یک یا دو.آستانه شناسایی پلاک جدید.با مشاهده تغییرات( شامل کاهش افزایش ، تغییر یا جابه جایی) کمتر از این آستانه در نویسه های پلاک، آن را تکراری قلمداد کن")]
        public byte char_diffrence { get; set; } = 1;

        [Category("ب) پارامترهای ویدیو"), DisplayName("ورود خودرو"), DefaultValue((byte)20)]
        [Description("آستانه‏ی تشخیص ورود خودرو\nبرای تصاویر شب، مقدار 7 و برای روز مقدار 20 مناسب است\nمقدار بزرگتر حساسیت کمتری دارد و خودروی کمتری تشخیص می دهد")]
        public byte diff_thresh { get; set; } = 15;         //Difference threshold between current frame and background to suppose entrance of new car 

        [Category("ب) پارامترهای ویدیو"), DisplayName("بافر پلاکها"), DefaultValue((byte)7), Description("برای پیشگیری از گزارش پلاکهای تکراری، این تعداد پلاک مشابه، بافر شده و سپس یکی گزارش می شود.")]
        public byte plate_buf_size { get; set; } = 40;      // Buffer length of recent successive plates (max = 50). 

        [Category("ا) پارامترهای پرکاربرد"), DisplayName("تشخیص چند پلاک"), DefaultValue(true), Description("اگر مجوز چند پلاکه خریده اید، با تیک زدن این پارامتر، می توانید چند پلاک را در یک تصویر بخوانید")]
        public bool detect_multi_plate { get; set; } = true;


        [Category("ب) پارامترهای ویدیو"), DisplayName("پخش صدا"), DefaultValue(false), Description("در حالت کار با وی ال سی، می توانید صدا را هم داشته باشید. برای برخی دوربینها حتما باید این تیک را بزنید که استریم ویدیو را دریافت کنید")]
        public bool play_audio_from_camera { get; set; } = false; //in vlc mode we can play audio (from version 7.45)

        [Category("ج) پارامترهای عمومی"), DisplayName("نوع پلاک"), DefaultValue((byte)0), Description("صفر: فقط پلاک استاندارد، یک: پلاک استاندارد + پلاک اروند، دو: پلاک استاندارد + پلاک ارگ")]
        public byte plate_type { get; set; } = 0;// 

        [Category("ب) پارامترهای ویدیو"), DisplayName("آستانه حداقل هیستوگرام"), DefaultValue((byte)50), Description("عددی بین 0 و 100 به منظور تشخیص محل خودرو و پردازش همان منطقه به جای کل تصویر")]
        public byte min_thresh_hist { get; set; } = 50;

        [Category("ب) پارامترهای ویدیو"), DisplayName("آستانه حداکثر هیستوگرام"), DefaultValue((byte)170), Description("عددی بین 100 و 200 به منظور تشخیص محل خودرو و پردازش همان منطقه به جای کل تصویر")]
        public byte max_thresh_hist { get; set; } = 170;

        [Category("ب) پارامترهای ویدیو"), DisplayName(" صرفه جویی در پردازنده "), DefaultValue((byte)0), Description("با فعال سازی این گزینه در صورت امکان در مصرف پردازنده صرفه جویی خواهد شد.")]
        public byte economy { get; set; } = 0;

        [Category("ب) پارامترهای ویدیو"), DisplayName(" رهگیری پلاک ها "), DefaultValue((byte)0), Description("با فعال سازیاین گزینه،در تصویر پلاک توسط یک نشانه دنبال می شود.")]
        public bool marker { get; set; } = true;

        [Category("ه) پارامترهای عیب یابی"), DisplayName("تکرار ویدیو"), DefaultValue(false), Description("برای اینکه سیستم را با یک فایل ویدیویی زیر بار بگذارید، این تیک را بزنید. با اتمام فایل، از نو شروع می شود.")]
        public bool repeat { get; set; } = true;

        [Category("ه) پارامترهای عیب یابی"), DisplayName("حالت دیباگ"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید.")]
        public byte debug_level { get; set; } = 0;

        [Category("د) پارامترهای پیشرفته"), DisplayName("کرنل میانه"), DefaultValue((byte)0), Description("فیلتر میانه با این ابعاد. برای عدم اعمال فیلتر، صفر وارد کنید")]
        public byte medianKernel { get; set; } = 0;      // (0: no kernel) (3, 5, 7 ... median kernel of this size)                                                                                                   //اگر عدد صفر انتخاب شود، فقط رشته پلاک و مستطیل آن گزارش شده و تصویر بریده شده پلاک ارسال نمی شود

        [Category("د) پارامترهای پیشرفته"), DisplayName("فیلتر نرم کننده"), DefaultValue((byte)0), Description("برای باینری کردن وفقی استفاده می شود. اگر تصویرتان سایه دارد 13 و 3 را امتحان کنید.")]
        public int[] blur_kernel { get; set; } = new int[2] { 13, 13 };
        [Category("د) پارامترهای پیشرفته"), DisplayName("عرض تصویر پلاک"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public int plate_width { get; set; } = 384;
        [Category("د) پارامترهای پیشرفته"), DisplayName("ارتفاع تصویر پلاک"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public int plate_height { get; set; } = 128;
        [Category("د) پارامترهای پیشرفته"), DisplayName("عرض تصویر"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public int frame_width { get; set; } = 416;
        [Category("د) پارامترهای پیشرفته"), DisplayName("ارتفاع تصویر"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public int frame_height { get; set; } = 352;
        [Category("د) پارامترهای پیشرفته"), DisplayName(" حداقل دقت پلاک زمان پردازش"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public float plate_confidence_min { get; set; } = 0.3f;
        [Category("د) پارامترهای پیشرفته"), DisplayName("استانه پلاک"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public float plate_threshold { get; set; } = 0.4f;
        [Category("د) پارامترهای پیشرفته"), DisplayName("حداقل دقت نویسه"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public float char_confidence_min { get; set; } = 0.4f;
        [Category("د) پارامترهای پیشرفته"), DisplayName("آستانه نویسه"), DefaultValue((byte)0), Description("مورد استفاده توسعه دهندگان کتابخانه است. تغییر ندهید")]
        public float char_threshold { get; set; } = 0.4f;
        [Category("ج) پارامترهای عمومی"), DisplayName("شناسایی پلاک فارسی"), DefaultValue((byte)1), Description("چنانچه نسخه از نسخه لاتین ساتپا استفاده می کنید و قصد تشخیص پلاک های ایرانی را ندارید مقدار صفر قرار دهید ")]
        public byte detect_persian_plate { get; set; } = 1;//
        [Category("ج) پارامترهای عمومی"), DisplayName("شناسایی پلاک لاتین"), DefaultValue((byte)0), Description("چنانچه نسخه از نسخه فارسی ساتپا استفاده می کنید و قصد تشخیص پلاک های لاتین را ندارید مقدار صفر قرار دهید ")]
        public byte detect_english_plate { get; set; } = 0;//
        [Category("ج) پارامترهای عمومی"), DisplayName("شناسایی پلاک عربی"), DefaultValue((bool)false), Description("چنانچه نسخه از نسخه فارسی ساتپا استفاده می کنید و قصد تشخیص پلاک های عربی را ندارید مقدار صفر قرار دهید ")]
        public bool detect_arbic_plate { get; set; } = false;//
        [Category("ج) پارامترهای عمومی"), DisplayName("کشور"), Description(" چنانچه از نسخه لاتین ساتپا استفاده می کنید و قصد شناسایی پلاک کشور خاصی را دارید از این گزینه کمک بگیرید. کشور ترکیه = 1")]
        public byte custom_country { get; set; } = 1;//

        [Category("د) پارامترهای پیشرفته"), DisplayName("پردازنده"), DefaultValue(0), Description(" در صورت تمایل به استفاده از پردازنده گرافیکی مقدار یک یا شش را قرار دهید ")]
        public byte processor { get; set; } = 0;//

    }


    public class SATPA_API
    {
        public delegate void satpa_EVENT_CALLBACK(int event_type, byte stream, int plt_idx);

        public const string DLL_NAME = "ANPR.dll";
        public const int WM_USER = 0x0400;
        public const int WM_NEW_FRAME = WM_USER + 100;
        public const int WM_SCENE_CHANGED = WM_USER + 101;
        public const int WM_PLATE_DETECTED = WM_USER + 102;
        public const int WM_PLATE_NOT_DETECTED = WM_USER + 103; //when a car is in the field of camera but its plate is not recognized
        public const int WM_END_OF_VIDEO = WM_USER + 104; //when video file finished or camera closed
        public const int WM_CONNECTED = WM_USER + 105; //Connected to camera (or video file) from Ver 8.43

        //هنگامی که اولین پلاک در صحنه دیده می شود، برای ترسیم مستطیل اطراف آن
        //رویداد تشخیص قطعی پلاک شماره 102 است
        public const int WM_INITIAL_PLATE = WM_USER + 108;
        public const int WM_CAM_NOT_FOUND = WM_USER + 109;

        public const int WM_CAM_SEARCH = WM_USER + 110;
        public const int WM_CAM_SEARCH_ERROR = WM_USER + 111;
        public const int WM_MESSAGE = WM_USER + 112;


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                                                                        //
        //                                                                                                                                        //
        //                                                        ERROR CODES                                                                     //
        //                                                                                                                                        //
        //                                                                                                                                        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public const int EXIT = 199;                                // در صورت دریافت این کد، نمونه های ساخته شده از کتابخانه ساتپا حذف خواهند شد و کتابخانه در دسترس نخواهد بود. 

        public const int ERR_INTERNAL = -3569;
        public const int ERR_MAX_INSTANCE = -3571;
        public const int ERR_DLL_IS_IN_USE = -3593;
        public const int ERR_INTERNAL2 = -4372;
        public const int ERR_INTERNAL3 = -13082;
        public const int ERR_INTERNAL4 = -13568;
        public const int ERR_INTERNAL5 = -14642;
        public const int ERR_INTERNAL6 = -16542;
        public const int ERR_INTERNAL7 = -2018151;
        public const int ERR_CFG_FILE = -137384;                //قایل مجوز یافت نشد
        public const int ERR_CFG_FILE2 = -143623;                //قایل مجوز برای این سیستم نیست
        public const int ERR_CFG_FILE3 = -162104;                //قایل مجوز برای این سیستم نیست
        public const int ERR_CFG_FILE4 = -165184;                //تاریخ سیسنم اشتباه است
        public const int ERR_CFG_FILE5 = -175387;                //قایل منقضی شده است
        public const int ERR_MAX_INSTANCE2 = -194970;
        public const int ERR_FATAL_100 = 100;
        public const int ERR_FATAL_101 = 101;                                // فایل مجوز مشکل دارد
        public const int ERR_FATAL_102 = 102;                                // فایل مجوز مشکل دارد
        public const int ERR_FATAL_103 = 103;                                // فایل مجوز مشکل دارد
        public const int ERR_FATAL_104 = 104;                                // فایل مجوز مشکل دارد
        public const int ERR_FATAL_105 = 105;                                // فایل مجوز مشکل دارد
        public const int ERR_FATAL_106 = 106;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_107 = 107;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_108 = 108;
        public const int ERR_FATAL_109 = 109;
        public const int ERR_FATAL_110 = 110;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_111 = 111;
        public const int ERR_FATAL_112 = 112;
        public const int ERR_FATAL_113 = 113;
        public const int ERR_FATAL_114 = 114;
        public const int ERR_FATAL_115 = 115;
        public const int ERR_FATAL_116 = 116;
        public const int ERR_FATAL_117 = 117;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_118 = 118;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_119 = 119;
        public const int ERR_FATAL_120 = 120;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_121 = 121;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_122 = 122;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_123 = 123;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_124 = 124;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_125 = 125;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_126 = 126;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_127 = 127;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_128 = 128;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_129 = 129;                                // مشکل در قفل سخت افزاری
        public const int ERR_FATAL_130 = 130;                                // مشکل در قفل سخت افزاری
        public const int ERR_MAX_INSTANCE3 = 131;
        public const int ERR_TRIAL_LICENSE_EXPIRED = 132;
        public const int ERR_INVALID_CODE = 133;                                 // کدی که در تابع کریت فرستاده شده است اشتباه است
        public const int ERR_INIALIZE_SDL = 134;                                 // خطا در مقدار دهی اولیه کتابخانه اس دی ال
        public const int ERR_MAX_FRAME = 135;                                // تعداد فریم های ازمایشی به اتمام رسیده است
        public const int ERR_MCS_FILE = 136;                                 // خطا در یافتن فایل های ام سی اس
        public const int ERR_PLAY_LICENSE = 137;                                 // مجوز شما امکان پلاک خوانی ندارد
        public const int ERR_SATPA_IS_IN_USE = 138;                              // 
        public const int ERR_TRAFFIC_FINISHED = 140;                                 // ترافیک شما به پایان رسیده است
        public const int ERR_BLOCKED_LISENCE = 141;                              // به دلیل تلاش ها ناموفق در شارژ دانگل، دانگل قفل شده است
        public const int ERR_UNKNOWN = 142;                              // خطای ناشناخته ای در حین شارژ دانگل رخ داده است.دانگل را مجدد وصل کنید
        public const int ERR_FATAL_143 = 143;                                // سریال شارژ معتبر نیست 
        public const int WAR_CHARGE_SUCCESSFULLY = 144;                              //  دانگل با موفقیت شارژ شد
        public const int ERR_CHARGE_UNSUCCESSFULLY = 145;                                //  خطا در شارژ دانگل
        public const int ERR_CHARGE_FUNCTION = 146;                              //  خطا در شارژ دانگل
        public const int ERR_UNKNOWN_CHARGE_FUNCTION = 147;                              //  خطا در شارژ دانگل
        public const int ERR_GENERATE_CODE = 148;                                //  خطا در تولید سریال فعال سازی
        public const int ERR_UNKNOWN_GENERATE_CODE = 149;                                //  خطا در تولید سریال فعال سازی
        public const int ERR_INCORRECT_DATE = 150;                               // تاریخ سیستم اشتباه است 
        public const int ERR_LICENSE_EXPIRED = 151;                              // مجوز شما منقضی شده است 
        public const int ERR_INCORRECT_HID_PATH = 152;                               // مسیر ذخیره اچ ای دی مشکل دارد
        public const int ERR_VIRTUAL = 153;                                // کتابخانه ساتپا امکان اجرا روی ویرچوال را ندارد
        public const int ERR_CFG_NOT_FOUND = 154;                                // فایل مجوز یافت نشد
        public const int ERR_EXIT_001 = 170;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_002 = 171;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_003 = 172;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_004 = 173;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_005 = 174;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_006 = 175;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int ERR_EXIT_007 = 176;                                // اشیا ساخته شده از ساتپا حذف خواهند شد
        public const int WAR_TRIAL_LICENSE_DAYS = 200;                               // برای مشاهده تعداد روز های باقی مانده از دوره ازمایشی از دمو کمک بگیرید 
        public const int ERR_RECOGNIZE_FUNCTION = 201;                               // خطا در تابع
        public const int ERR_INSTANCE_NOT_CREATED = 202;                                 // ابتدا باید یک شی از ساتپا ساخته شود
        public const int ERR_START_FUNCTION = 203;                               // خطا در تابع استارت
        public const int ERR_STARTVLC_FUNCTION = 204;                                // خطا در تابع استارت VLC
        public const int ERR_RECORDING_FUNCTION = 205;                               // خطا در تابع استارت VLC
        public const int ERR_SDL_RENDERER = 206;                                 // خطا SDL
        public const int ERR_STOP_RECORDING_FUNCTION = 207;                              // خطا در تابع استارت VLC
        public const int ERR_MATRIX = 208;                               // خطا در هدر ماتریکس
        public const int ERR_CMATRIX = 209;                              // خطا در تابع سازنده CMATRIX
        public const int ERR_CREATE_TEXTURE = 210;                               // خطا در SDL DRAW
        public const int ERR_INCORRECT_SIZE = 211;                               // خطا در SDL DRAW
        public const int WAR_TRIAL = 212;                                // خطا در SDL DRAW
        public const int WAR_ABOUT = 213;                                // خطا در SDL DRAW
        public const int ERR_NORMAL_TRAFFIC = 214;                               // دانگل دارای شارژ کافی است و امکان شارژ مجدد وجود ندارد
        public const int WAR_TEMP = 215;
        public const int ACTIVATION_CODE = 216;                              // برای دریافت سریال فعال سازی از دمو کمک بگیرید
        public const int WAR_LICENSE_FINISHING = 217;                                // مجوز پلاک خوانی شما رو به اتمام است
        public const int ERR_ACTIVATION_CODE = 218;                                // مجوز شما دارای اعتبار کافی است
        public const int ERR_satpa_set_params = 219;



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                                                                        //
        //                                                                                                                                        //
        //                                                        FUNCTIONS                                                                       //
        //                                                                                                                                        //
        //                                                                                                                                        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <typeparam name="Type"></typeparam>

        public class OptionalInput<Type>
        {
            public Type Result { get; set; }
        }
        //1
        //تابع زیر به ازای هر نسخه کتابخانه (مثلا به ازای هر دوربین) حتما باید یکبار فراخوانی شود. 
        //این تابع شبکه های عصبی مورد استفاده را بارگذاری می کند
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_create(byte instance, byte per_plate_license, [MarshalAs(UnmanagedType.LPWStr)] string security_code, byte log_level = 1, [MarshalAs(UnmanagedType.LPWStr)] string cfg_file = null, int reserve = 0);

        //1-1
        //این تابع مدیریت تمام رویدادهای کتابخانه را بر عهده دارد
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_set_event_callback(satpa_EVENT_CALLBACK callback_fcn, int reserve = 0, int reserve2 = 0);

        //2
        //این تابع مسیر فایل تصویری را دریافت کرده و نتیجه را بر می گرداند: 
        //رشته، میزان اطمینان به رشته حاصله و مستطیل پلاک
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognize(byte instance, [MarshalAs(UnmanagedType.LPWStr)] string fn,
            [MarshalAs(UnmanagedType.LPWStr)] string result, ref float cnf, ref RECT prc);

        //3
        //این تابع مانند تابع بالایی است با این تفاوت که اندیس مستطیل مورد علاقه را هم می گیرد.
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognizeROI(byte instance, byte roi_idx, [MarshalAs(UnmanagedType.LPWStr)] string fn,
            [MarshalAs(UnmanagedType.LPWStr)] string result, ref float cnf, ref RECT prc);


        //4
        //تابع زیر برای بافری است که از دوربین یا فایل گرفته اید و نوعا یک جریان فشرده مثل جی پگ است.
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognize_stream(byte instance, IntPtr compressed_stream, int size, [MarshalAs(UnmanagedType.LPWStr)] string result, ref float cnf, ref RECT prc);

        //5
        //تابع زیر برای زمانی است که بایتهای تصویر به صورت فشرده نشده در آرایه ای قرار دارند
        //مثلا اشاره گر ابتدای یک بیت مپ
        //مثال آن در همین برنامه دیده می شود
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognize_buffer(byte instance, IntPtr bytes, int W, int H, int step, [MarshalAs(UnmanagedType.LPWStr)] string result, ref float cnf, ref RECT prc);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognize_bufferROI(byte instance, byte roi_idx, IntPtr bytes, int W, int H, int step, [MarshalAs(UnmanagedType.LPWStr)] string result, ref float cnf, ref RECT prc);

        //6
        //خروجی تابع 2 یک رشته فارسی یونیکد است، اگر خروجی انگلیسی «اسکی» را لازم دارید از این تابع استفاده کنید 
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_get_ascii_result([MarshalAs(UnmanagedType.LPWStr)] string result_fa, [MarshalAs(UnmanagedType.LPStr)] string result_en);//Get ascii results in English

        //7
        //خروجی تابع 2 یک رشته فارسی یونیکد است، اگر خروجی انگلیسی «یونیکد» را لازم دارید از این تابع استفاده کنید 
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_get_en_result([MarshalAs(UnmanagedType.LPWStr)] string result_fa, [MarshalAs(UnmanagedType.LPWStr)] string result_en);//Get unicode results in English

        //8
        //یافتن نویسه ها از بافر حافظه ای که تنها شامل تصویر پلاک است
        //به عبارتی محل پلاک باید قبلا یافت شده باشد
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_find_chars(byte instance, IntPtr bytes, int W, int H, int step, RECT roi, [MarshalAs(UnmanagedType.LPWStr)] string result, ref float pcnf);

        //9
        //این تابع برای تنظیم پارامترهای کتابخانه است.
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_set_params(byte instance, [MarshalAs(UnmanagedType.LPStr)] string slpr_params);

        //این تابع برای تست برخی پارامترهای کتابخانه است و حتی الامکان نباید استفاده شود.
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_set_debug_mode(byte instance, byte debug_level);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_add_ROI(byte instance, [MarshalAs(UnmanagedType.LPStr)] string roi_json);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_clear_ROIs(byte instance);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_get_plate(byte instance, int plate_idx, [MarshalAs(UnmanagedType.LPWStr)] string result);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_get_missed_car(byte instance, int car_idx, [MarshalAs(UnmanagedType.LPWStr)] string result);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_about([MarshalAs(UnmanagedType.LPWStr)] string result);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_charge_license([MarshalAs(UnmanagedType.LPStr)] string serial);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_get_activation_code();

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_start_grabbing(byte instance, [MarshalAs(UnmanagedType.LPStr)] string URL, float interval_ms, IntPtr hwndDraw, byte take_shots, byte draw_method);
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_stop_grabbing(byte instance);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_start_grabbingVLC(byte instance, [MarshalAs(UnmanagedType.LPStr)] string URL, float interval_ms, IntPtr hwndDraw, byte take_shots, byte draw_method);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_stop_grabbingVLC(byte instance);


        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_pause_or_resume(byte instance, byte pause);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_get_frame_info(byte instance, ref int W, ref int H, ref int channels, ref int step);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern IntPtr satpa_get_frame(byte instance);

        //Start Processing of Camera Frames
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_start_process(byte instance, bool plate_marker = true);

        //Stop Processing of Camera Frames
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_stop_process(byte instance);
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_start_recording(byte instance, [MarshalAs(UnmanagedType.LPStr)] string path);
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_stop_recording(byte instance);

        //Recognize Last Frame Grabbed from camera or video file
        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_recognize_cur_frame(byte instance, [MarshalAs(UnmanagedType.LPWStr)] string str, ref RECT pr, ref float cnf);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern void satpa_camera_search();

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern int satpa_get_cam([MarshalAs(UnmanagedType.LPWStr)] string camera, byte idx);

        [System.Runtime.InteropServices.DllImport(DLL_NAME)]
        public static extern short satpa_get_lic();

        public class SLPRParams
        {
            public int[] num_valid_chars { get; set; }// Number of valid characters usuallay {8, 0}. if e.g. 5 character plates are also available, use {8, 5}
            public byte medianKernel { get; set; }       // (0: no kernel) (3, 5, 7 ... median kernel of this size)
            public short vlc_net_cache_time { get; set; } // vlc caching time default is 1000 ms

            //Video Related Params
            public byte n_frm_skip_on_success { get; set; } //Number of frames to be skipped after successful plate detection
            public byte difference_threshold { get; set; }        //Difference threshold between current frame and background to suppose entrance of new car 
            public byte plate_buffer_size { get; set; }     // Buffer length of recent successive plates (max = 50). 
            public int skip_same_plate_frame { get; set; }// don't report same plate until "some frame" elpased
            public byte detect_multi_plate { get; set; } //اگر مجوز چند پلاکه فعال باشد و این گزینه هم 1 باشد، امکان گزارش چند پلاک هست
            public byte play_audio_from_camera { get; set; } //in vlc mode we can play audio (from version 7.45)

            public byte plate_type { get; set; } //0: Only Iran standard, 1: + Arvand, 2: + Arg 
            public byte report_non_standard_plates { get; set; } // 

            // از این دو پارامتر برای تغییر (کوچک و بزرگ کردن) ناحیه پردازش استفاده میشود
            public byte min_threshold_hist { get; set; }
            public byte max_threshold_hist { get; set; }

            //تنظیمات پیشرفته که به ندرت نیاز است تغییر داده شود
            public int[] blur_kernel { get; set; } //Size of blur kernel used for binarization. Default is 13x13. To handle shadow, try 13x1 or 13x3	
            public byte char_diffrence { get; set; }
            public byte economy { get; set; }
            public byte detect_english_plate { get; set; }
            public byte detect_persian_plate { get; set; }
            public byte custom_country { get; set; }
            //پیشرفته
            public int plate_width { get; set; }
            public int plate_height { get; set; }
            public int frame_width { get; set; }
            public int frame_height { get; set; }
            public float plate_confidence_min { get; set; }
            public float plate_threshold { get; set; }
            public float char_confidence_min { get; set; }
            public float char_threshold { get; set; }
            public byte process_on_gpu { get; set; }
            public byte gstreamer { get; set; }
            public bool tcp { get; set; }
            public bool detect_arbic_plate { get; set; }
            public int missed_car_threshold { get; set; }
            public float x_coefficient_missed_car_area { get; set; }
            public float y_coefficient_missed_car_area { get; set; }

        };

        public class SMissedCar
        {
            private IntPtr  _frame_pointer;
            public long frame { get; set; }//image pointer
            public IntPtr frame_pointer
            {
                get
                {
                    _frame_pointer = new System.IntPtr(frame);
                    return _frame_pointer;
                }
            }
            public int car_width { get; set; }
            public int car_height { get; set; }
            public int frame_width { get; set; }
            public int frame_height { get; set; }
            public int left { get; set; }
            public int top { get; set; }
        }


        public class SPlateResult
        {
            private string _plate_string, _plate_english_string;
            private IntPtr _plate_image_pointer, _car_image_pointer;
            public int[] plate_string_unicode_indices { get; set; }
            public string plate_string
            {
                get
                {
                    _plate_string = "";
                    for (int i = 0; i < plate_string_unicode_indices.Length; i++)
                        _plate_string = _plate_string + Convert.ToChar(plate_string_unicode_indices[i]);
                    return _plate_string;
                }
                set
                {
                    _plate_string = value;
                }
            }
            public int[] plate_english_string_unicode_indices { get; set; }
            public string plate_english_string
            {
                get
                {
                    _plate_english_string = "";
                    for (int i = 0; i < plate_english_string_unicode_indices.Length; i++)
                        _plate_english_string = _plate_english_string + Convert.ToChar(plate_english_string_unicode_indices[i]);
                    return _plate_english_string;
                }
                set
                {
                    _plate_english_string = value;
                }

            }
            public int plate_width { get; set; }
            public int plate_height { get; set; }
            public int plate_left { get; set; }
            public int plate_top { get; set; }
            public float confidence { get; set; }
            public long plate_image { get; set; }//image pointer
            public long car_image { get; set; }//image pointer
            public IntPtr plate_image_pointer 
            { get
                {
                    _plate_image_pointer = new System.IntPtr(plate_image);
                    return _plate_image_pointer;
                }
            }
            public IntPtr car_image_pointer
            {
                get
                {
                    _car_image_pointer = new System.IntPtr(car_image);
                    return _car_image_pointer;
                }
            }
            public byte direction { get; set; }//DIR_UNKNOWN = 0, DIR_COMMING = 1, DIR_DEPARTING = 2
            public byte n_char { get; set; }//تعداد کل نویسه ها (ارقام و حروف)
            public byte n_letter { get; set; }//تعداد حروف یافت شده در پلاک
            public byte count { get; set; }//چند بار یک پلاک در فریمهای مختلف تکرار شده است
            public byte roi { get; set; }//در کدام ناحیه مورد علاقه، این پلاک یافت شده است
            public byte line_number { get; set; }
            public byte is_english { get; set; }
            public byte country { get; set; }
        };

        public class Lic_Info
        {
            private string _owner_name_string, _product_name_string;
            public int[] owner_name { get; set; }
            public string owner_name_string {
                get
                {
                    for (int i = 0; i < owner_name.Length; i++)
                        _owner_name_string = _owner_name_string + Convert.ToChar(owner_name[i]);
                    return _owner_name_string;
                }
            }
            public int[] product_name { get; set; }
            public string product_name_string {
                get
                {
                    for (int i = 0; i < product_name_string.Length; i++)
                        _product_name_string = _product_name_string + Convert.ToChar(product_name[i]);
                    return _product_name_string;
                }
            }
            public string product_version_string { get; set; }
            public int is_trial { get; set; }
            public int for_developer { get; set; }
            public int start_date { get; set; }
            public int end_date { get; set; }
            public int camera_count { get; set; }
            public int multi_plate_supported { get; set; }
            public int read_limit_plate { get; set; }
            public int country_number { get; set; }
        }

        public class camera_info
        {
            string _url_string,_ip_string;
            public int[] url { get; set; }
            public string url_string { 
                get
                {
                    for (int i = 0; i < url.Length; i++)
                        _url_string = _url_string + Convert.ToChar(url[i]);
                    return _url_string;
                }
                set
                {
                    _url_string = value;
                }
            }
            public int[] ip { get; set; }
            public string ip_string 
            {
                get
                {
                    for (int i = 0; i < ip.Length; i++)
                        _ip_string = _ip_string + Convert.ToChar(ip[i]);
                    return _ip_string;
                }
                set
                {
                    _ip_string = value;
                }
            }
            public int port { get; set; }
        };

        public struct RECT
        {
            public int left { get; set; }
            public int top { get; set; }
            public int right { get; set; }
            public int bottom { get; set; }
        };

    }

}
