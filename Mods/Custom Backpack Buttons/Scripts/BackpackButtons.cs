using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using System.Xml;
using SDX.Core;
using System;

class XUiC_BackpackWindowSDX : XUiC_BackpackWindow
{
    static bool showDebugLog = false;

	// These designate the names of the buttons for the XUi controller
	private XUiController btnStashAll;
	private XUiController btnStashAllButFirst;
	private XUiController btnStashMatch;

    // Reflection references
    private FieldInfo itemControllers;
    private FieldInfo placeSoundField;
    private new FieldInfo viewComponent;

    public static void DebugMsg(string msg)
    {
        if (showDebugLog)
        {
            UnityEngine.Debug.Log(msg);
        }
    }
	
	public static bool AddItem(ItemStack _itemStack, XUi _xui)
	{
		if (_xui.lootContainer == null)
		{
			return false;
		}
		_xui.lootContainer.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && _xui.lootContainer.AddItem(_itemStack);
	}
	
	// This sets up the buttons to actually work when poked. Also required so they can be called by XML
    public override void Init()
    {
        base.Init();
        this.btnStashAll = base.GetChildById("btnStashAll");
        this.btnStashAll.OnPress += this.ButtonStashAll_OnPressed;
        this.btnStashAllButFirst = base.GetChildById("btnStashAllButFirst");
        this.btnStashAllButFirst.OnPress += this.ButtonStashAllButFirst_OnPressed;
		this.btnStashMatch = base.GetChildById("btnStashMatch");
        this.btnStashMatch.OnPress += this.ButtonStashMatch_OnPressed;


        // Adds a local reference for the following private fields. These are only references to the variables, not the actual data.
        itemControllers = typeof(XUiC_ItemStackGrid).GetField("itemControllers", BindingFlags.NonPublic | BindingFlags.Instance);
        placeSoundField = typeof(XUiC_ItemStack).GetField("placeSound", BindingFlags.NonPublic | BindingFlags.Instance);
        viewComponent = typeof(XUiC_ItemStackGrid).GetField("viewComponent", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public void HandleMove(XUiC_ItemStack xuic_ItemStack, FieldInfo placeSoundField)
    {
        AudioClip valueBefore = (AudioClip)placeSoundField.GetValue(xuic_ItemStack);
        placeSoundField.SetValue(xuic_ItemStack, null);

        xuic_ItemStack.HandleMoveToPreferredLocation();

        placeSoundField.SetValue(xuic_ItemStack, valueBefore);
    }


    // And this is all the code required to make the buttons actually DO something
    public void ButtonStashAll_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        XUiC_ItemStackGrid xuiC_ItemStackGrid = (XUiC_ItemStackGrid)base.GetChildByType<XUiC_ItemStackGrid>();

        // Using GetValue() to actually retrieve the value in the itemControllers field.
        XUiController[] slots = itemControllers.GetValue(xuiC_ItemStackGrid) as XUiController[];
        
        for (int i = 0; i < slots.Length; i++)
            HandleMove((XUiC_ItemStack)slots[i], placeSoundField);
    }
	
    public void ButtonStashAllButFirst_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        XUiC_ItemStackGrid xuiC_ItemStackGrid = (XUiC_ItemStackGrid)base.GetChildByType<XUiC_ItemStackGrid>();
        XUiController[] slots = itemControllers.GetValue(xuiC_ItemStackGrid) as XUiController[];
        XUiV_Grid xuiv_Grid = (XUiV_Grid)viewComponent.GetValue(xuiC_ItemStackGrid);
        for (int i = xuiv_Grid.Columns; i < slots.Length; i++)
            HandleMove((XUiC_ItemStack)slots[i], placeSoundField);
    }

    public void ButtonStashMatch_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        ItemStack[] backpackslots = this.xui.PlayerInventory.GetBackpackItemStacks();
        XUiC_ItemStackGrid xuiC_ItemStackGrid = (XUiC_ItemStackGrid)base.GetChildByType<XUiC_ItemStackGrid>();
        XUiController[] slots = itemControllers.GetValue(xuiC_ItemStackGrid) as XUiController[];

		for (int i = 0; i < slots.Length; i++)
			{
			    if ( this.xui.lootContainer != null )
					if (this.xui.lootContainer.HasItem(backpackslots[i].itemValue))
					HandleMove((XUiC_ItemStack)slots[i], placeSoundField);
				    
				if ( this.xui.vehicle != null && this.xui.vehicle.bag != null )
					if (this.xui.vehicle.lootContainer.HasItem(backpackslots[i].itemValue))
					HandleMove((XUiC_ItemStack)slots[i], placeSoundField);
					// GameManager.ShowTooltip(player, Localization.Get("NoVehicleStack"));
			}

    }
}	