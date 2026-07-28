using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Runtime.InteropServices.ComTypes;
using static VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras.Interop
{
    class SATPA
    {
        public PictureBox picLive_view;
        public Button box;
        public byte stream_number, debug_mode, draw_method = 0;
        public int frame_height = 0, frame_width = 0;
        public string name, url;
        public SLPRPropertyGrid satpa_settings;
        public int frame_w = 0, frame_h, frame_step = 0, frame_ch = 0;
        public List<plate> plte_buffer = new List<plate>();
        public List<missed_car> missed_buffer = new List<missed_car>();
        public string grabbing = "no", play_mode = "vlc"; //indicates whether we are grabbing or not: 0 --> not grabbing, 1 regular grabbing, 2 VLC grabbing 
        public string is_in_process = "no";
        public int frame_counter = 0, empty_frame_counter = 0, plate_counter = 0, not_detect_counter = 0;
        public bool save = false, maximized = false, is_in_record = false;
        public static short max_roi_number = 4;
        public List<RECT> satpa_rois = new List<RECT>();
        public System.Drawing.Pen pen_rect = new System.Drawing.Pen(System.Drawing.Color.Purple, 10);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                                                       //
        //                                                                                                                       //
        //                                                                                                                       //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Bitmap frame; //bitmap of playing frames on picture control (in video mode)
        private SLPRParams prm = new SLPRParams();
        private string backup_grabbing = "no";
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public SATPA(byte stream, string camera_name, PictureBox picBox, License license)
        {
            stream_number = stream;
            name = camera_name;
            picLive_view = picBox;
            satpa_create(stream_number, (byte)license, "www.shahaab-co.com 02331099", 2);
        }

        public byte play(string url)
        {
            auto_process(satpa_settings.marker);
            plate_counter = 0;
            frame_counter = 0;
            not_detect_counter = 0;
            this.url = url;
            frame_height = 0;
            frame_width = 0;
            SetParams();
            float interval = (byte)(1000 / satpa_settings.frame_rate);
            //rtsp://admin:admin@192.168.55.160:554/h264
            byte take_shots = satpa_settings.take_shots_from_camera ? (byte)1 : (byte)0;
            grabbing = "vlc";
            if (play_mode == "vlc")
            {
                if (satpa_start_grabbingVLC(stream_number, url, interval, picLive_view.Handle, take_shots, draw_method) < 0)
                {   
                    grabbing = "no";
                    return 0;
                }
            }
            else
            {
                if (satpa_start_grabbing(stream_number, url, interval, picLive_view.Handle, take_shots, draw_method) < 0)
                {
                    grabbing = "no";
                    return 0;
                }
            }

            return 1;
        }

        public byte stop()
        {
            stop_auto_process();
            Thread.Sleep(250);
            grabbing = "no";
            if (play_mode == "vlc")
                satpa_stop_grabbingVLC(stream_number);
            else
                satpa_stop_grabbing(stream_number);
            picLive_view.Image = null;
            frame = null;
            delete_all_roi();
            satpa_rois.Clear();
            Thread.Sleep(100);
            return 1;
        }

        private void StopEveryThing()
        {
            satpa_stop_process(stream_number);
            //یک ثانیه وقفه به منظور اتمام گزارش پلاکهای احتمالی
            for (int i = 0; i < 100; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            satpa_stop_grabbing(stream_number);
            satpa_stop_grabbingVLC(stream_number);
            grabbing = "no";
        }
        private void SetParams()
        {
            prm.plate_buffer_size = satpa_settings.plate_buf_size;
            prm.num_valid_chars = satpa_settings.num_valid_chars;
            prm.detect_multi_plate = satpa_settings.detect_multi_plate ? (byte)1 : (byte)0;
            prm.difference_threshold = satpa_settings.diff_thresh;
            prm.n_frm_skip_on_success = satpa_settings.n_frm_skip_on_success;
            prm.skip_same_plate_frame = satpa_settings.skip_same_plate_frm;
            prm.vlc_net_cache_time = satpa_settings.vlc_net_cache_time;
            prm.plate_type = satpa_settings.plate_type;
            prm.report_non_standard_plates = satpa_settings.report_non_standard_plates ? (byte)1 : (byte)0;
            prm.medianKernel = satpa_settings.medianKernel;
            prm.play_audio_from_camera = satpa_settings.play_audio_from_camera ? (byte)1 : (byte)0;
            prm.min_threshold_hist = satpa_settings.min_thresh_hist;
            prm.max_threshold_hist = satpa_settings.max_thresh_hist;
            prm.blur_kernel = satpa_settings.blur_kernel;
            prm.economy = satpa_settings.economy;
            prm.char_diffrence = satpa_settings.char_diffrence;
            prm.detect_persian_plate = satpa_settings.detect_persian_plate;
            prm.detect_english_plate = satpa_settings.detect_english_plate;
            prm.custom_country = satpa_settings.custom_country;
            prm.plate_width = satpa_settings.plate_width;
            prm.plate_height = satpa_settings.plate_height;
            prm.frame_width = satpa_settings.frame_width;
            prm.frame_height = satpa_settings.frame_height;
            prm.plate_confidence_min = satpa_settings.plate_confidence_min;
            prm.plate_threshold = satpa_settings.plate_threshold;
            prm.char_confidence_min = satpa_settings.char_confidence_min;
            prm.char_threshold = satpa_settings.char_threshold;
            prm.process_on_gpu = satpa_settings.processor;
            prm.gstreamer = satpa_settings.gstreamer;
            prm.tcp = satpa_settings.tcp;
            prm.detect_arbic_plate = satpa_settings.detect_arbic_plate;
            prm.missed_car_threshold = satpa_settings.missed_car_threshold;
            prm.x_coefficient_missed_car_area = satpa_settings.x_coefficient_missed_car_area;
            prm.y_coefficient_missed_car_area = satpa_settings.y_coefficient_missed_car_area;
            var settings_json = JsonSerializer.Serialize(prm);
            satpa_set_params(stream_number, settings_json);

        }

        public void palteNotDetected(int plt_idx)
        {
            for (int i = 0; i < plt_idx;i++)
            {
            string str = new string(' ', 2000);
            satpa_get_missed_car(stream_number, i,str);
                var car = JsonSerializer.Deserialize<SMissedCar>(str);
               RECT rc = new RECT();
                rc.left = car.left; 
                rc.top = car.top;
                rc.right = car.left + car.car_width;
                rc.bottom = car.top + car.car_height;
                missed_car missed = new missed_car();
                missed.frame = (Bitmap)make_pic_plate(car.frame_pointer, car.frame_height, car.frame_width)/*.Clone()*/;
                missed.rc = rc ;
                missed_buffer.Add(missed);

            }
        }

        public void UpdateResults(int plt_idx)//for video ,not image
        {
            string str = new string(' ', 2000);
            satpa_get_plate(stream_number, plt_idx, str);
            var plate = JsonSerializer.Deserialize<SPlateResult>(str);
            plate new_plate = new plate();
            //رشته پلاک را پس پردازش کنید و با رشته های قبل مقایسه کنید
            //پلاک جدید را با چند پلاک اخیر مقایسه کنید و درصورت وجود تفاوت کمتر از یک یا دو کاراکتر، این دو پلاک یکی هستند و احتمالا به دلیل نزدیک شدن یا دور شدن از دوربین رشته پلاک تغییر پیدا کرده است. در این موارد پلاکی که دقت بالاتری دارد را نگه دارید.
            if (plate.confidence < satpa_settings.min_cnf)
                return;
            new_plate.splate_result = plate;
            /*if (plate.n_char <= 3)
                return;*/
            new_plate.result_en = plate.plate_english_string;
            if (plate.car_image_pointer != null && plate.plate_image_pointer != null)
            {
                new_plate.car_pic = (Bitmap)new Bitmap(frame_w, frame_h, frame_step, PixelFormat.Format24bppRgb, plate.car_image_pointer)/*.Clone()*/;
                new_plate.plate_pic = (Bitmap)make_pic_plate(plate.plate_image_pointer, plate.plate_height, plate.plate_width)/*.Clone()*/;
            }
            new_plate.cnf = plate.confidence;
            plte_buffer.Add(new_plate);
            plate_counter++;
            if (plte_buffer.Count > 10)
                plte_buffer.RemoveAt(0);
        }

        public Bitmap make_pic_plate(IntPtr pic, int h, int w)
        {
            Bitmap img = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            if (img == null)
                return img;
            BitmapData dataDst = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr dst = dataDst.Scan0;
            IntPtr src = pic;
            int step = 3 * img.Width;
            BitmapData dataSrc = null;

            for (int i = 0; i < img.Height; i++)
            {
                CopyMemory(dst, src, (uint)img.Width * 3);
                src += step;
                dst += dataDst.Stride;
            }
            //if (dataSrc != null)
            //    car_img.UnlockBits(dataSrc);
            img.UnlockBits(dataDst);
            return img;
        }

        public short auto_process(bool marker)
        {
            short res = satpa_start_process(stream_number, marker);
            is_in_process = "yes";
            return res;
        }

        public short stop_auto_process()
        {
            short res = satpa_stop_process(stream_number);
            is_in_process = "no";
            return res;
        }

        public void save_setting()
        {
            SetParams();
            satpa_set_debug_mode(0, satpa_settings.debug_level);
        }

        public void UpdateFrame()
        {
            if (draw_method != 3)
            {
                //در حالتهای 0 تا 2، فریمها توسط کتابخانه ترسیم می شود
                //لذا اینجا فقط شمارنده فریم را به روز رسانی می کنیم
                frame_counter++;
                return;
            }
            if ((frame_w > 0) && (grabbing != "no"))
            {
                //vlpr_pause_or_resume(1); //Prevent changing frame in C++ while showing it here in C#
                IntPtr pFrame = satpa_get_frame(0);
                if (pFrame == IntPtr.Zero)
                {
                    empty_frame_counter++;
                    //vlpr_pause_or_resume(0);
                    if (empty_frame_counter > 10)
                    {
                        StopEveryThing();
                    }
                    return;
                }
                frame_counter++;
                empty_frame_counter = 0;
                if (frame == null)
                {
                    frame = new Bitmap(frame_w, frame_h, frame_step, PixelFormat.Format24bppRgb, pFrame);
                    picLive_view.Image = frame;
                }

            }
            picLive_view.Invalidate();

        }

        public void pause()
        {
            backup_grabbing = grabbing;
            satpa_pause_or_resume(stream_number, 1);
            is_in_process = "pause";
            grabbing = "no";
        }

        public void resume()
        {
            satpa_pause_or_resume(stream_number, 0);
            is_in_process = "yes";
            grabbing = backup_grabbing;
        }

        public void record(string path)
        {
            is_in_record = true;
            satpa_start_recording(stream_number, path);
        }
        public void stop_record()
        {
            is_in_record = false;
            satpa_stop_recording(stream_number);
        }

        public Bitmap get_frame()
        {
            if (frame_w < 10 || frame_h < 10)
                throw new System.ArgumentException("Width or height are not valid.\nPlease replay camera");
            IntPtr src = satpa_get_frame(stream_number);
            Bitmap frame = new Bitmap(frame_w, frame_h, frame_step, PixelFormat.Format24bppRgb, src);
            return frame;
        }

        public void set_roi(List<RECT> new_roi)
        {
            if (grabbing == "no")
            {
                MessageBox.Show("برای اضافه کردن ناحیه، دوریبن باید در حال نمایش باشد");
                return;
            }
            if (new_roi.Count() > max_roi_number)
            {
                MessageBox.Show("امکان ایجاد ناحیه بیشتر وجود ندارد");
                return;
            }
            satpa_rois.Clear();
            for (int i = 0; i < new_roi.Count; i++)
                satpa_rois.Add(new_roi[i]);
            satpa_clear_ROIs(stream_number);
            for (int i = 0; i < satpa_rois.Count(); i++)
            {
                var roi_json = JsonSerializer.Serialize(satpa_rois[i]);
                satpa_add_ROI(stream_number, roi_json);

            }
        }


        public void delete_all_roi()
        {
            satpa_clear_ROIs(stream_number);
            satpa_rois.Clear();
        }

        private Rectangle RECT2Rectangle(RECT rect)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.X = rect.left;
            rectangle.Y = rect.top;
            rectangle.Width = rect.right - rect.left;
            rectangle.Height = rect.bottom - rect.top;
            return rectangle;
        }
        public void WriteLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log" + stream_number.ToString() + ".txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                //meta log
            }
        }
    }
    public class plate
    {
        public SPlateResult splate_result;
        public Bitmap plate_pic;
        public Bitmap car_pic;
        public string result_en;
        public float cnf;
    }

    public class missed_car
    {
        public Bitmap frame;
        public RECT rc;
    }

    public enum draw_method
    {
        win_api = 0,
        opengl,
        sdl,
        draw_on_host
    }
    public enum License
    {
        per_camera = 0,
        per_traffic = 0
    }
}
