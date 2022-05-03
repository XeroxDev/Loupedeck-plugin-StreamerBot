namespace Loupedeck.StreamerBotPlugin
{
    using System;

    using Services;

    public class StreamerBotPlugin : Plugin
    {
        public override Boolean HasNoApplication => true;
        public override Boolean UsesApplicationApiOnly => true;

        public override void Load()
        {
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