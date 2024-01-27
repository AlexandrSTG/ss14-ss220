using Content.Shared.CrewManifest;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.CrewManifest;

[GenerateTypedNameReferences]
public sealed partial class CrewManifestUi : DefaultWindow
{
    private CrewManifestEntries? _entryCache;

    public CrewManifestUi()
    {
        RobustXamlLoader.Load(this);

        StationName.AddStyleClass("LabelBig");

        TextFilter.OnTextEntered += e => Populate(StationName.Text ?? "", _entryCache);
        TextFilter.OnTextChanged += e => Populate(StationName.Text ?? "", _entryCache);
    }

    public void Populate(string name, CrewManifestEntries? entries)
    {
        CrewManifestListing.DisposeAllChildren();
        CrewManifestListing.RemoveAllChildren();

        StationNameContainer.Visible = entries != null;
        StationName.Text = name;

        _entryCache = entries;

        if (entries == null)
            return;

        var entryList = FilterEntries(entries);
        CrewManifestListing.AddCrewManifestEntries(entryList);
    }

    private CrewManifestEntries FilterEntries(CrewManifestEntries entries)
    {
        if (string.IsNullOrWhiteSpace(TextFilter.Text))
        {
            return entries;
        }

        var result = new CrewManifestEntries();
        foreach (var entry in entries.Entries)
        {
            if (entry.Name.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase)
                || entry.JobPrototype.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase)
                || entry.JobTitle.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase))
            {
                result.Entries.Add(entry);
            }
        }

        return result;
    }
}
