using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomAvatar.UI
{
    public class AvatarListViewController : BSMLResourceViewController, TableView.IDataSource
    {
        public override string ResourceName => "CustomAvatar.Views.AvatarListViewController.bsml";
		
        [UIComponent("avatar-list")] public CustomListTableData avatarList;
        [UIComponent("up-button")] public Button upButton;
        [UIComponent("down-button")] public Button downButton;

        private List<CustomAvatar> avatars = new List<CustomAvatar>();
        private LevelListTableCell tableCellTemplate;
        
        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            AvatarManager.Instance.AvatarChanged += OnAvatarChanged;

            if (firstActivation) FirstActivation();
        }

        private void FirstActivation()
        {
	        tableCellTemplate = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => x.name == "LevelListTableCell");
	        AvatarManager.Instance.GetAvatarsAsync(avatar =>
	        {
		        Plugin.Logger.Info("Loaded avatar " + avatar.descriptor.Name);

		        avatars.Add(avatar);

		        ReloadData();
	        }, ex =>
	        {
				Plugin.Logger.Error("Failed to load avatar: " + ex.Message);
	        });

	        avatarList.tableView.dataSource = this;
	        avatarList.tableView.SetPrivateField("_pageUpButton", upButton);
	        avatarList.tableView.SetPrivateField("_pageDownButton", downButton);

	        TableViewScroller scroller = avatarList.tableView.GetPrivateField<TableViewScroller>("_scroller");

			upButton.onClick.AddListener(() =>
			{
				scroller.PageScrollUp();
				avatarList.tableView.RefreshScrollButtons();
			});

			downButton.onClick.AddListener(() =>
			{
				scroller.PageScrollDown();
				avatarList.tableView.RefreshScrollButtons();
			});
        }

        protected override void DidDeactivate(DeactivationType deactivationType)
        {
	        base.DidDeactivate(deactivationType);

	        AvatarManager.Instance.AvatarChanged -= OnAvatarChanged;
        }

        [UIAction("avatar-click")]
        private void OnAvatarClicked(TableView table, int row)
        {
	        AvatarManager.Instance.SwitchToAvatar(avatars[row]);
        }

        private void OnAvatarChanged(SpawnedAvatar avatar)
        {
	        ReloadData();
        }

        private void ReloadData()
        {
	        avatars.Sort((a, b) => string.Compare(a.descriptor.Name, b.descriptor.Name, StringComparison.CurrentCulture));

	        int currentRow = avatars.FindIndex(a => a.fullPath == AvatarManager.Instance.CurrentlySpawnedAvatar?.CustomAvatar.fullPath);
			
            avatarList.tableView.ReloadData();
            avatarList.tableView.ScrollToCellWithIdx(currentRow, TableViewScroller.ScrollPositionType.Center, true);
            avatarList.tableView.SelectCellWithIdx(currentRow);
        }

        public float CellSize()
        {
	        return 8.5f;
        }

        public int NumberOfCells()
        {
	        return avatars.Count;
        }

        public TableCell CellForIdx(TableView tableView, int idx)
        {
	        LevelListTableCell tableCell = avatarList.tableView.DequeueReusableCellForIdentifier("AvatarListCell") as LevelListTableCell;

	        if (!tableCell)
	        {
		        tableCell = Instantiate(tableCellTemplate);

		        foreach (var image in tableCell.GetPrivateField<UnityEngine.UI.Image[]>("_beatmapCharacteristicImages"))
		        {
			        DestroyImmediate(image);
		        }

		        tableCell.SetPrivateField("_beatmapCharacteristicAlphas", new float[0]);
		        tableCell.SetPrivateField("_beatmapCharacteristicImages", new UnityEngine.UI.Image[0]);

		        tableCell.reuseIdentifier = "CustomAvatarsTableCell";
	        }

	        CustomAvatar avatar = avatars[idx];

	        tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText").text = avatar.descriptor.Name;
	        tableCell.GetPrivateField<TextMeshProUGUI>("_authorText").text = avatar.descriptor.Author;
	        tableCell.GetPrivateField<RawImage>("_coverRawImage").texture = avatar.descriptor.Cover?.texture ?? Texture2D.blackTexture;

	        return tableCell;
        }
    }
}
