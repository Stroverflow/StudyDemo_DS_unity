using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    /**
     * 库存类
     * 用来管理物品拾取、武器装备切换等功能
     */
    public class PlayerInventory : MonoBehaviour
    {
        private WeaponSlotManager _weaponSlotManager;
        
        public WeaponItem rightWeapon;
        public WeaponItem leftWeapon;

        private void Awake()
        {
            _weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
        }

        private void Start()
        {
            _weaponSlotManager.LoadWeaponOnSlot(rightWeapon, false);
            _weaponSlotManager.LoadWeaponOnSlot(leftWeapon, true);
        }
    }
}

