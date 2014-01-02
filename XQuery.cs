using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using unirest_net.http;

namespace XQuery
{
    public class XQuery
    {
        private View View { get; set; }
        private Activity Activity { get; set; }
        private View CurrentView { get; set; }
        private readonly Boolean _useActivity;
        private readonly Dictionary<int, Bitmap> _imageCache = new Dictionary<int, Bitmap>();

        public XQuery(View view)
        {
            _useActivity = false;
            View = view;
        }

        public XQuery(Activity activity)
        {
            _useActivity = true;
            Activity = activity;
        }

        public XQuery Id(int id)
        {
            CurrentView = GetView(id);
            return this;
        }

        public View GetView(int id)
        {
            return _useActivity ? Activity.FindViewById(id) : View.FindViewById(id);
        }

        public T GetView<T>(int id) where T : View
        {
            return _useActivity ? Activity.FindViewById<T>(id) : View.FindViewById<T>(id);
        }

        public XQuery Text(String text)
        {
            ((TextView)CurrentView).Text = text;
            return this;
        }

        public XQuery Image(Drawable drawable)
        {
            ((ImageView)CurrentView).SetImageDrawable(drawable);
            return this;
        }

        public XQuery Image(int resId)
        {
            ((ImageView)CurrentView).SetImageResource(resId);
            return this;
        }

        public XQuery Image(Bitmap bitmap)
        {
            ((ImageView)CurrentView).SetImageBitmap(bitmap);
            return this;
        }

        public XQuery Image(String url)
        {
            ThreadPool.QueueUserWorkItem(e =>
            {
                var response = Unirest.get(url).asBinary();
                var bitmap = BitmapFactory.DecodeStream(response.Body);
                CurrentView.Post(() => ((ImageView)CurrentView).SetImageBitmap(bitmap));
            });

            return this;
        }

        public XQuery Image(String url, Boolean memCache)
        {
            var hash = url.GetHashCode();

            if (_imageCache.ContainsKey(hash))
                return Image(_imageCache[hash]);

            ThreadPool.QueueUserWorkItem(e =>
            {
                var response = Unirest.get(url).asBinary();
                var bitmap = BitmapFactory.DecodeStream(response.Body);
                _imageCache.Add(hash, bitmap);
                Image(bitmap);
            });

            return this;
        }
    }
}
