using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Simemes.Treasures;

namespace Simemes.UI
{
    public class StolenInfo : MonoBehaviour
    {
        [SerializeField] private UIChestPanel _chestPanel;
        [SerializeField] private Text _name;
        [SerializeField] private Text _title;
        [SerializeField] private Image _photo;
        [SerializeField] private List<UIChestSlot> _slots;


        [SerializeField] private Button _stealBtn;
        [SerializeField] private GameObject _popupFrame;
        [SerializeField] private Text _popupText;

        // for local Test ======================================================
        [SerializeField] private Sprite[] _images;
        private string[] _names = {
            "Calvina",
            "Kensey",
            "Martelli",
            "Neo",
            "Sonel",
            "Behramji",
            "Dashiell",
            "Mikala",
            "Siofra",
            "Kamiyah",
            "Cissy",
            "Barossa",
            "Spirit",
            "Barbra",
            "Takashia",
            "Rihanna",
            "Ainhoa",
            "Telemachus",
            "Vanna",
            "Derry",
        };

        private string[] _titles = {
            "Farmer",
            "Worker",
            "Clerk",
            "Technician",
            "Teacher",
            "Police Officer",
            "Sales Manager",
            "Department Supervisor",
            "Factory Manager",
            "Bank Manager",
            "Local Councilor",
            "Mayor",
            "Member of Parliament",
            "Governor",
            "Cabinet Member/Minister",
            "Deputy Prime Minister/Vice President",
            "Prime Minister",
            "Presidential Assistant",
            "Vice President",
            "President"
        };

        public class TreasureData
        {
            public float RemainTime;
            public bool HasBuff;
        }

        public class PlayerData
        {
            public int Sprite;
            public int Name;
            public int Titles;
            public System.DateTime LastUpdate;
            public List<TreasureData> Treasures = new List<TreasureData>();
        }

        private List<PlayerData> _playerRecord = new List<PlayerData>();
        // ===============================================================

        private int _chestCount;
        private int _playerDataIndex = 0;

        private void OnEnable()
        {
            LoadNewInfo();
        }

        private void Set(string name, string title, Sprite sprite)
        {
            _name.text = name;
            _title.text = title;
            _photo.sprite = sprite;
            _stealBtn.interactable = true;
        }

        public void Steal()
        {
            if (_chestPanel.TryGetEmptySlot(out UIChestSlot empty))
            {
                bool success;
                bool hasBuff;

                int index = Random.Range(0, _chestCount);
                UIChestSlot slot = _slots[index];
                hasBuff = slot.Content.HasBuff;
                success = hasBuff ? false : Random.Range(0, 100) < 75;

                if (hasBuff)
                {
                    _stealBtn.interactable = false;
                }

                if (success)
                {
                    slot.gameObject.SetActive(false);

                    // remove treasure data
                    PlayerData playerData = _playerRecord[_playerDataIndex];
                    playerData.Treasures.RemoveAt(index);

                    if (_chestPanel != null)
                    {
                        _chestPanel.AddChest(_chestPanel.Slots.IndexOf(empty));

                        empty.SetBox(slot.Content);
                        empty.Seal();
                    }
                }
                _popupText.text = hasBuff ? "Trigger guard!\n<color=red>Steal failed...</color>" : success ? "Steal succeeded!" : "<color=red>Steal failed...</color>"; ;// "偷取失敗\n觸發防護罩" : success ? "成功偷取" : "偷取失敗";
            }
            else
            {
                _popupText.text = "Your chest is full";//"你的寶箱已滿";
            }


            _popupFrame.SetActive(true);
        }

        public void LoadNextInfo()
        {
            if (_playerDataIndex < _playerRecord.Count - 1)
                LoadInfoBase(++_playerDataIndex);
            else
                LoadNewInfo();
        }

        public void LoadPreviousInfo()
        {
            int playerDataIndex = _playerDataIndex - 1;
            if (playerDataIndex > -1)
            {
                _playerDataIndex = playerDataIndex;
                LoadInfoBase(_playerDataIndex);
            }
        }

        private void LoadChestData()
        {
            _chestCount = Random.Range(1, _slots.Count);
            for (int i = 0; i < _slots.Count; ++i)
            {
                bool enable = i < _chestCount;
                UIChestSlot slot = _slots[i];
                slot.gameObject.SetActive(enable);
                if (enable)
                {
                    var treasureBoxConfig = TreasureSystem.instance.GetTreasureBoxConfig(0);
                    if (treasureBoxConfig == null)
                        return;

                    var treasureBox = new TreasureBox(treasureBoxConfig);
                    treasureBox.RemainTime = Random.Range(1000, 86400);

                    slot.SetBox(treasureBox);

                    if (Random.Range(0, 100) < 50)
                        slot.AddBuff(TreasureSystem.instance.GetBuff(1));

                    slot.Seal();
                }
            }
        }

        private void LoadChestData(PlayerData playerData)
        {
            System.DateTime now = System.DateTime.Now;
            _chestCount = playerData.Treasures.Count;
            for (int i = 0; i < _slots.Count; ++i)
            {
                bool enable = i < _chestCount;
                UIChestSlot slot = _slots[i];
                slot.gameObject.SetActive(enable);
                if (enable)
                {
                    var treasureBoxConfig = TreasureSystem.instance.GetTreasureBoxConfig(0);
                    if (treasureBoxConfig == null)
                        return;

                    TreasureData treasureData = playerData.Treasures[i];
                    var treasureBox = new TreasureBox(treasureBoxConfig);
                    treasureBox.RemainTime = (float)(treasureData.RemainTime - (now - playerData.LastUpdate).TotalSeconds);

                    slot.SetBox(treasureBox);

                    if (treasureData.HasBuff)
                        slot.AddBuff(TreasureSystem.instance.GetBuff(1));

                    slot.Seal();
                }
            }
        }

        private void LoadNewInfo()
        {
            // for local Test ======================================================
            int nameIndex = Random.Range(0, _names.Length);
            string name = _names[nameIndex];
            string title = _titles[nameIndex];
            int spriteIndex = nameIndex < _images.Length ? nameIndex : _images.Length - 1;
            Sprite sprite = _images[spriteIndex];
            LoadChestData();

            // log player data
            PlayerData playerData = new PlayerData() { Name = nameIndex, Titles = nameIndex, Sprite = spriteIndex, LastUpdate = System.DateTime.Now };
            for (int i = 0; i < _chestCount; ++i)
            {
                UIChestSlot slot = _slots[i];
                playerData.Treasures.Add(new TreasureData() { RemainTime = slot.Content.RemainTime, HasBuff = slot.Content.HasBuff });
            }
            _playerRecord.Add(playerData);
            _playerDataIndex = _playerRecord.Count - 1;

            // ===============================================================

            Set(name, title, sprite);
        }

        private void LoadInfoBase(int index)
        {
            PlayerData playerData = _playerRecord[index];
            string name = _names[playerData.Name];
            string title = _titles[playerData.Titles];
            Sprite sprite = _images[playerData.Sprite];
            LoadChestData(playerData);
            Set(name, title, sprite);
        }
    }
}