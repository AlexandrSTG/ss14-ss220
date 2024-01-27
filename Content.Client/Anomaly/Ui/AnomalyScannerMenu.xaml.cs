﻿using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Anomaly.Ui;

[GenerateTypedNameReferences]
public sealed partial class AnomalyScannerMenu : FancyWindow
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public FormattedMessage LastMessage = new();
    public TimeSpan? NextPulseTime;

    public AnomalyScannerMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    public void UpdateMenu()
    {
        var msg = new FormattedMessage(LastMessage);

        if (NextPulseTime != null)
        {
            msg.PushNewline();
            msg.PushNewline();
            var time = NextPulseTime.Value - _timing.CurTime;
            var timestring = $"{time.Minutes:00}:{time.Seconds:00}";
            msg.AddMarkup(Loc.GetString("anomaly-scanner-pulse-timer", ("time", timestring)));
        }

        TextDisplay.SetMarkup(msg.ToMarkup());
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (NextPulseTime != null)
            UpdateMenu();
    }
}
