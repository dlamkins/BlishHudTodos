﻿using Blish_HUD.Controls;

namespace TodoList.Components
{
    public sealed class TodoEntry : FlowPanel
    {
        private readonly TodoEntryHeader _header;

        public TodoEntry(Resources resources, Settings settings, int width)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            HeightSizingMode = SizingMode.AutoSize;
            Width = width;

            _header = new TodoEntryHeader(resources, settings, width) { Parent = this };
        }

        protected override void DisposeControl()
        {
            _header.Dispose();
            base.DisposeControl();
        }
    }
}