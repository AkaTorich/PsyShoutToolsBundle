using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace PsyTrancePlayer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Enable hardware acceleration for better performance
        RenderOptions.ProcessRenderMode = RenderMode.Default;

        // Optional: Force GPU rendering tier if available
        if (RenderCapability.Tier >= 0x00020000)
        {
            // GPU Tier 2 or higher available - use hardware acceleration
        }
    }
}
