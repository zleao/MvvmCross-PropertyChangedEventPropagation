using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;

namespace PropertyChangedEventPropagation.Droid
{
    [Activity(
		Label = "PropertyChangedEventPropagation.Droid"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}