using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    /**
     * 游戏物品类
     */
    public class Item : ScriptableObject
    {
        [Header("Item Information")]
        public Sprite itemIcon;
        public string itemName;
    }
}

