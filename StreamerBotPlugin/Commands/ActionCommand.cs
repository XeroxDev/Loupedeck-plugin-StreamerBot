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

    using Services;

    using Action = Models.Receive.Action;

    public class ActionCommand : PluginDynamicCommand
    {
        private readonly HttpService _httpService;
        private Action[] _actions;

        public ActionCommand() : base("Action", "Choose and execute your actions", "Action")
        {
            this._httpService = HttpService.Instance;
            this._actions = this._httpService.GetActions().Actions;
            this.MakeProfileAction("tree");
        }

        protected override PluginProfileActionData GetProfileActionData()
        {
            this._actions = this._httpService.GetActions().Actions;
            var tree = new PluginProfileActionTree("Select Windows Settings Application");

            tree.AddLevel("Category");
            tree.AddLevel("Command");

            var node = tree.Root.AddNode("Actions");

            foreach (var item in this._actions.Where(action => action.Enabled))
            {
                node.AddItem(item.Id, item.Name, $"Execute {item.Name}");
            }

            return tree;
        }

        protected override void RunCommand(String actionParameter)
        {
            var action = this._actions.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter);

            if (action == null)
            {
                return;
            }

            this._httpService.ExecuteAction(action.Id);
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            this._actions.FirstOrDefault(a => actionParameter is not null && a.Id == actionParameter)?.Name ?? base.GetCommandDisplayName(actionParameter, imageSize);
    }
}