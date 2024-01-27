// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.Message;
using Content.Shared.SS220.CriminalRecords;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.SS220.CriminalRecords.UI;

[GenerateTypedNameReferences]
public sealed partial class CriminalRecordDisplay : PanelContainer
{
    private int? _time;
    private CriminalRecordsWindow? _main;

    public CriminalRecordDisplay()
    {
        RobustXamlLoader.Load(this);

        DeleteDialog.MouseFilter = MouseFilterMode.Stop;
        DeleteButton.OnPressed += ToggleDeleteDialog;
        DeleteCancel.OnPressed += ToggleDeleteDialog;
        DeleteConfirm.OnPressed += args =>
        {
            if (_time.HasValue && _main != null)
                _main.DeleteRecord(_time.Value);

            ToggleDeleteDialog(args);
        };
    }

    private void ToggleDeleteDialog(BaseButton.ButtonEventArgs _)
    {
        DeleteDialog.Visible = !DeleteDialog.Visible;
    }

    public void Setup(Texture? icon, CriminalStatusPrototype? status, string message, int time, bool isLast, CriminalRecordsWindow mainWindow)
    {
        _time = time;
        _main = mainWindow;

        if (icon != null)
            CriminalStatusIcon.Texture = icon;
        else
            CriminalStatusIcon.Visible = false;

        var formattedTime = TimeSpan.FromSeconds(time).ToString(@"hh\:mm");

        // We should use FormattedMessage, otherwise users may add markup to their message and crash parser
        FormattedMessage msg = new();

        if (status != null)
        {
            msg.PushColor(status.Color);
            msg.AddText(status.Name + ": ");
            msg.Pop();
        }

        if (isLast)
        {
            RecordTime.SetMarkup($"[color={Color.White.ToHex()}]{formattedTime}[/color]");
            msg.PushColor(Color.White);
            RecordMessage.SetMessage(msg);
        }
        else
        {
            RecordTime.SetMarkup(formattedTime);
        }

        msg.AddText(message);
        msg.Pop();
        RecordMessage.SetMessage(msg);
    }
}
