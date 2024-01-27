using System.Linq;
using System.Numerics;
using Content.Client.Resources;
using Content.Client.Viewport;
using Content.Shared.DeviceNetwork;
using Content.Shared.SurveillanceCamera;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.SurveillanceCamera.UI;

[GenerateTypedNameReferences]
public sealed partial class SurveillanceCameraMonitorWindow : DefaultWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    // SS220 Camera-Map begin
    private enum TabNumbers
    {
        CameraMap,
        CameraList
    }

    public void SetMap(ResPath mapPath)
    {
        MapViewer.ViewedPicture = mapPath;
    }

    private string _subnetFilter = "";
    private Dictionary<string, Dictionary<string, (string, Vector2)>> _camerasCache = new();
    // SS220 Camera-Map end

    public event Action<string>? CameraSelected;
    public event Action<string>? SubnetOpened;
    public event Action? CameraRefresh;
    public event Action? SubnetRefresh;
    public event Action? CameraSwitchTimer;
    public event Action? CameraDisconnect;

    private string _currentAddress = string.Empty;
    private string _currentName = string.Empty; // SS220 Camera-Map
    private bool _isSwitching;
    private readonly FixedEye _defaultEye = new();
    private readonly Dictionary<string, int> _subnetMap = new();

    /*
    private string? SelectedSubnet
    {
        get
        {
            if (SubnetSelector.ItemCount == 0
                || SubnetSelector.SelectedMetadata == null)
            {
                return null;
            }

            return (string) SubnetSelector.SelectedMetadata;
        }
    }
    */

    public SurveillanceCameraMonitorWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        // This could be done better. I don't want to deal with stylesheets at the moment.
        var texture = _resourceCache.GetTexture("/Textures/Interface/Nano/square_black.png");
        var shader = _prototypeManager.Index<ShaderPrototype>("CameraStatic").Instance().Duplicate();

        CameraView.ViewportSize = new Vector2i(500, 500);
        CameraView.Eye = _defaultEye; // sure
        CameraViewBackground.Stretch = TextureRect.StretchMode.Scale;
        CameraViewBackground.Texture = texture;
        CameraViewBackground.ShaderOverride = shader;

        SubnetList.OnItemSelected += OnSubnetListSelect;

        SubnetSelector.OnItemSelected += args =>
        {
            _subnetFilter = (string) args.Button.GetItemMetadata(args.Id)!; // SS220 Camera-Map
            PopulateCameraList(_camerasCache); // SS220 Camera-Map
            SubnetSelector.Select(args.Id);
        };
        SubnetRefreshButton.OnPressed += _ => SubnetRefresh!();
        SubnetRefreshButton2.OnPressed += _ => SubnetRefresh!(); // SS220 Camera-Map
        CameraRefreshButton.OnPressed += _ => CameraRefresh!();
        CameraDisconnectButton.OnPressed += _ => CameraDisconnect!();

        // SS220 Camera-Map begin
        SelectorTabs.SetTabTitle((int) TabNumbers.CameraList, "Список");
        SelectorTabs.SetTabTitle((int) TabNumbers.CameraMap, "Карта");

        MapViewerControls.AttachToViewer(MapViewer);
        // SS220 Camera-Map end
    }


    // The UI class should get the eye from the entity, and then
    // pass it here so that the UI can change its view.
    public void UpdateState(IEye? eye, HashSet<string> subnets, string activeAddress, Dictionary<string, Dictionary<string, (string, Vector2)>> cameras)
    {
        _currentAddress = activeAddress;
        _currentName = string.Empty; //SS220 Camera-Map

        if (subnets.Count == 0)
        {
            SubnetSelector.AddItem(Loc.GetString("surveillance-camera-monitor-ui-no-subnets"));
            SubnetSelector.Disabled = true;
        }

        if (SubnetSelector.Disabled && subnets.Count != 0)
        {
            SubnetSelector.Clear();
            SubnetSelector.Disabled = false;
        }

        // if the subnet count is unequal, that means
        // we have to rebuild the subnet selector
        if (SubnetSelector.ItemCount != subnets.Count)
        {
            SubnetSelector.Clear();
            _subnetMap.Clear();

            foreach (var subnet in subnets)
            {
                var id = AddSubnet(subnet);
                _subnetMap.Add(subnet, id);
            }
        }

        PopulateCameraList(cameras);
        MapViewer.Populate(cameras);
        SetCameraView(eye);
    }

    private void PopulateCameraList(Dictionary<string, Dictionary<string, (string, Vector2)>> cameras)
    {
        SubnetList.Clear();

        // SS220 Camera-Map begin
        _camerasCache = cameras;

        foreach (var (subnetFreqId, subnetCameras) in cameras)
        {
            foreach (var (address, (name, _)) in subnetCameras)
            {
                if (address == _currentAddress)
                    _currentName = name;

                if (subnetFreqId == _subnetFilter)
                    AddCameraToList(name, address);
            }
        }
        // SS220 Camera-Map end

        SubnetList.SortItemsByText();
    }

    private void SetCameraView(IEye? eye)
    {
        var eyeChanged = eye != CameraView.Eye || CameraView.Eye == null;
        CameraView.Eye = eye ?? _defaultEye;
        CameraView.Visible = !eyeChanged && !_isSwitching;
        CameraDisconnectButton.Disabled = eye == null;

        if (eye != null)
        {
            if (!eyeChanged)
            {
                return;
            }

            _isSwitching = true;
            CameraViewBackground.Visible = true;
            CameraStatus.Text = Loc.GetString("surveillance-camera-monitor-ui-status",
                ("status", Loc.GetString("surveillance-camera-monitor-ui-status-connecting")),
                ("address", _currentAddress)) + $" ({_currentName})"; // SS220 Camera-Map
            CameraSwitchTimer!();
        }
        else
        {
            CameraViewBackground.Visible = true;
            CameraStatus.Text = Loc.GetString("surveillance-camera-monitor-ui-status-disconnected");
        }
    }

    public void OnSwitchTimerComplete()
    {
        _isSwitching = false;
        CameraView.Visible = CameraView.Eye != _defaultEye;
        CameraViewBackground.Visible = CameraView.Eye == _defaultEye;
        CameraStatus.Text = Loc.GetString("surveillance-camera-monitor-ui-status",
                            ("status", Loc.GetString("surveillance-camera-monitor-ui-status-connected")),
                            ("address", _currentAddress)) + $" ({_currentName})"; // SS220 Camera-Map
    }

    private int AddSubnet(string subnet)
    {
        var name = subnet;
        if (_prototypeManager.TryIndex<DeviceFrequencyPrototype>(subnet, out var frequency))
        {
            name = Loc.GetString(frequency.Name ?? subnet);
        }

        SubnetSelector.AddItem(name);
        SubnetSelector.SetItemMetadata(SubnetSelector.ItemCount - 1, subnet);

        if (_subnetFilter == "")
            _subnetFilter = subnet;

        return SubnetSelector.ItemCount - 1;
    }

    private void AddCameraToList(string name, string address)
    {
        var item = SubnetList.AddItem($"{name}: {address}");
        item.Metadata = address;
    }

    private void OnSubnetListSelect(ItemList.ItemListSelectedEventArgs args)
    {
        CameraSelected!((string) SubnetList[args.ItemIndex].Metadata!);
    }
}
