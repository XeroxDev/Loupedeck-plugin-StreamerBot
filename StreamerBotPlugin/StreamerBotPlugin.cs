namespace Loupedeck.StreamerBotPlugin
{
    using System;

    using Services;

    public class StreamerBotPlugin : Plugin
    {
        internal static StreamerBotPlugin Instance { get; private set; }
        public override Boolean HasNoApplication => true;
        public override Boolean UsesApplicationApiOnly => true;

        private const String RESOURCE_PATH = "Loupedeck.StreamerBotPlugin.Resources";

        public StreamerBotPlugin() => Instance = this;

        public override void Load()
        {            
            this.Info.Icon16x16 = EmbeddedResources.ReadImage($"{RESOURCE_PATH}.Icons.sblogo16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage($"{RESOURCE_PATH}.Icons.sblogo32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage($"{RESOURCE_PATH}.Icons.sblogo48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage($"{RESOURCE_PATH}.Icons.sblogo256.png");
            var service = HttpService.Instance;
            service.OnSuccess += (_, _) =>
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Working...", "");
            };
            service.OnFailure += (_, _) =>
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Failed", "Connection error.");
            };
        }

        public override void Unload()
        {
        }

        private void OnApplicationStarted(Object sender, EventArgs e)
        {
        }

        private void OnApplicationStopped(Object sender, EventArgs e)
        {
        }

        public override void RunCommand(String commandName, String parameter)
        {
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {
        }
    }
}