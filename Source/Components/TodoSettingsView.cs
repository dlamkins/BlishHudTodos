﻿using System;
using System.Collections.ObjectModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Todos.Source.Components.Generic;
using Todos.Source.Utils;
using Todos.Source.Utils.Reactive;

namespace Todos.Source.Components
{
    public class TodoSettingsView : View
    {
        private readonly SettingsModel _settings;
        
        private FlowPanel _leftPanel;
        private IDisposable _showWindowOnMap;
        private IDisposable _backgroundOpacity;
        private IDisposable _opacityWhenNotFocussed;
        private IDisposable _alwaysShowWindow;
        private IDisposable _fixatedWindow;
        
        private FlowPanel _rightPanel;
        private IDisposable _toggleWindowHotkey;
        private IDisposable _checkboxType;

        public TodoSettingsView(SettingsModel settings)
        {
            _settings = settings;
        }

        protected override void Build(Container buildPanel)
        {
            _leftPanel = new FlowPanel
            {
                Parent = buildPanel,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = buildPanel.Width / 2, 
                Height = buildPanel.Height,
                OuterControlPadding = new Vector2(10, 10)
            };

            _alwaysShowWindow = AddBooleanSetting(_leftPanel, _settings.AlwaysShowWindow, "Always visible",
                "Whether or not the Todos window should also be shown during\r\ncutscenes, the character selection screen and loading screens");
            _showWindowOnMap = AddBooleanSetting(_leftPanel, _settings.ShowWindowOnMap, "Show on map", 
                "Whether or not the Todos window should\r\nalso be shown while the map is opened");
            _fixatedWindow = AddBooleanSetting(_leftPanel, _settings.FixatedWindow, "Fixated Window",
                "When fixated, the Todos window can neither be moved nor resized");
            _backgroundOpacity = AddSliderSetting(_leftPanel, _settings.BackgroundOpacity,
                "Background opacity", "The opacity of the window background");
            _opacityWhenNotFocussed = AddSliderSetting(_leftPanel, _settings.WindowOpacityWhenNotFocussed,
                "Unfocused opacity", "The opacity of the window when you're not currently using it");
            _checkboxType = AddDropdownSetting(_leftPanel, _settings.CheckboxType, "Checkbox Type", 
                "The visual appearance of the checkboxes of todo entries"); 

            _rightPanel = new FlowPanel
            {
                Parent = buildPanel,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = buildPanel.Width / 2,
                Height = buildPanel.Height,
                OuterControlPadding = new Vector2(10, 10),
                Location = new Point(buildPanel.Width / 2, 0)
            };

            _toggleWindowHotkey = AddKeybindingSetting(_rightPanel, _settings.ToggleWindowHotkey, "Show/Hide Window",
                "Maximizes or minimizes the Todos window");
            
            base.Build(buildPanel);
        }

        private IDisposable AddKeybindingSetting(Container parent, IVariable<KeyBinding> setting, string label, string tooltip = null)
        {
            var row = new KeybindingAssigner(setting.Value) { Parent = parent, KeyBindingName = label, BasicTooltipText = tooltip };
            
            var interactionHandler = new EventHandler<EventArgs>((sender, e) => setting.Value = row.KeyBinding);
            row.BindingChanged += interactionHandler;
            
            setting.Subscribe(this, newValue => row.KeyBinding = newValue);
            
            return new SimpleDisposable(() =>
            {
                row.BindingChanged -= interactionHandler;
                setting.Unsubscribe(this);
            });
        }
        
        private IDisposable AddBooleanSetting(Container parent, IVariable<bool> setting, string label, string tooltip = null)
        {
            var row = TodoEditRow.For(parent, new Checkbox { Checked = setting.Value }, label, tooltip);
            
            var interactionHandler = new EventHandler<CheckChangedEvent>((sender, e) => setting.Value = e.Checked);
            row.CheckedChanged += interactionHandler;
            
            setting.Subscribe(this, newValue => row.Checked = newValue);
            
            return new SimpleDisposable(() =>
            {
                row.CheckedChanged -= interactionHandler;
                setting.Unsubscribe(this);
            });
        }
        
        private IDisposable AddSliderSetting(Container parent, IVariable<float> setting, string label, string tooltip = null)
        {
            var row = TodoEditRow.For(parent, new TrackBar { Value = setting.Value, MinValue = 0, MaxValue = 1, SmallStep = true }, label, tooltip);
            
            var interactionHandler = new EventHandler<ValueEventArgs<float>>((sender, e) => setting.Value = e.Value);
            row.ValueChanged += interactionHandler;
            
            setting.Subscribe(this, newValue => row.Value = newValue);
            
            return new SimpleDisposable(() =>
            {
                row.ValueChanged -= interactionHandler;
                setting.Unsubscribe(this);
            });
        }
        
        private IDisposable AddDropdownSetting<T>(Container parent, IVariable<T> setting, string label, string tooltip = null) where T : Enum
        {
            var row = TodoEditRow.For(parent, new Dropdown { SelectedItem = setting.Value.ToString()}, label, tooltip);
            foreach (var name in Enum.GetNames(typeof(T)))
                row.Items.Add(name);
            
            var interactionHandler = new EventHandler<ValueChangedEventArgs>((sender, e) => setting.Value = (T) Enum.Parse(typeof(T), e.CurrentValue));
            row.ValueChanged += interactionHandler;
            
            setting.Subscribe(this, newValue => row.SelectedItem = newValue.ToString());
            
            return new SimpleDisposable(() =>
            {
                row.ValueChanged -= interactionHandler;
                setting.Unsubscribe(this);
            });
        }

        protected override void Unload()
        {
            _alwaysShowWindow.Dispose();
            _showWindowOnMap.Dispose();
            _backgroundOpacity.Dispose();
            _opacityWhenNotFocussed.Dispose();
            _fixatedWindow.Dispose();
            _checkboxType.Dispose();
            _leftPanel.Dispose();

            _toggleWindowHotkey.Dispose();
            _rightPanel.Dispose();

            base.Unload();
        }
    }
}