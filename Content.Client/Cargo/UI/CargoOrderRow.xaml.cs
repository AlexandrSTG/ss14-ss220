﻿using Content.Shared.Cargo;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Cargo.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class CargoOrderRow : PanelContainer
    {
        public CargoOrderData? Order { get; set; }

        public CargoOrderRow()
        {
            RobustXamlLoader.Load(this);
        }
    }
}
