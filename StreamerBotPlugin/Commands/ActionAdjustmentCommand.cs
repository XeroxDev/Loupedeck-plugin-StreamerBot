// This file is part of the StreamerBotPlugin project.
// 
// Copyright (c) 2022 Dominic Ris
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Loupedeck.StreamerBotPlugin.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    using Services;

    using Action = Models.Receive.Action;

    public class ActionAdjustmentCommand : PluginDynamicAdjustment
    {
        private readonly HttpService _httpService;
        private Action[] _actions;

        private static readonly ConcurrentDictionary<String, Int32> _ActionAdjustmentValues = new ConcurrentDictionary<String, Int32>();

        public ActionAdjustmentCommand() : base("Adjustment Action", "Choose and execute your actions with adjustments (Reset sets it to 0)", "Adjustment Action", true)
        {
            this._httpService = HttpService.Instance;
            this._setActions().Wait();
            this.MakeProfileAction("tree");
        }

        private async Task<Action[]> _setActions() =>
            this._actions = (await this._httpService.GetActions()).Actions;


        protected override PluginProfileActionData GetProfileActionData()
        {
            this._setActions().Wait();
            var tree = new PluginProfileActionTree("Select Windows Settings Application");

            tree.AddLevel("Category");
            tree.AddLevel("Command");

            this._actions.GroupBy(x => x.Group).ToList().ForEach(actionGroup =>
            {
                var node = tree.Root.AddNode(actionGroup.Key);

                foreach (var item in actionGroup.Where(action => action.Enabled))
                {
                    if (!_ActionAdjustmentValues.ContainsKey(item.Id))
                    {
                        _ActionAdjustmentValues.TryAdd(item.Id, 0);
                    }

                    node.AddItem(item.Id, item.Name, $"Execute {item.Name}");
                }
            });

            return tree;
        }

        protected override void RunCommand(String actionParameter)
        {
            var action = this._actions?.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter);

            if (action != null)
            {
                _ActionAdjustmentValues.AddOrUpdate(action.Id, 0, (key, oldValue) => 0);
                this._httpService.ExecuteActionValue(action.Id, 0).Wait();
                this.AdjustmentValueChanged(actionParameter);
            }
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var action = this._actions?.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter);

            if (action != null)
            {
                _ActionAdjustmentValues.AddOrUpdate(action.Id, diff, (key, oldValue) =>
                {
                    var retVal = Clamp(oldValue + diff, 0, 100);

                    if (retVal == 100 && oldValue == 0)
                    {
                        return retVal = 0;
                    }

                    if (retVal != oldValue)
                    {
                        this._httpService.ExecuteActionValue(action.Id, diff).Wait();
                        this.AdjustmentValueChanged(actionParameter);
                    }

                    return retVal;
                });
            }
        }

        public static Int32 Clamp(Int32 value, Int32 min, Int32 max) =>
            (value < min) ? min : (value > max) ? max : value;

        protected override String GetAdjustmentValue(String actionParameter) =>
            _ActionAdjustmentValues.ContainsKey(actionParameter ?? String.Empty) ? $"({_ActionAdjustmentValues[actionParameter]})" : $"(0)";


        protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) =>
            this._actions?.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter)?.Name
                ?? base.GetCommandDisplayName(actionParameter, imageSize);

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            this._actions?.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter)?.Name
                ?? base.GetCommandDisplayName(actionParameter, imageSize);
    }
}