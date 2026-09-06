using Foundation;
using UIKit;

namespace MakeReady;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
	{
		MauiProgram.Checkpoint("AppDelegate.FinishedLaunching - entry, before base call");
		var result = base.FinishedLaunching(application, launchOptions);
		MauiProgram.Checkpoint("AppDelegate.FinishedLaunching - after base call");
		return result;
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
