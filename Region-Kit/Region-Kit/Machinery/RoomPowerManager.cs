using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static RegionKit.Machinery.MachineryStatic;
using static UnityEngine.Mathf;

namespace RegionKit.Machinery
{
    public class RoomPowerManager : UpdatableAndDeletable
    {
        public RoomPowerManager(Room rm, PlacedObject pobj)
        {
            var h = rm.GetHashCode();
            if (ManagersByRoom.ContainsKey(h)) ManagersByRoom[h] = this;
            else ManagersByRoom.Add(h, this);
            PO = pobj;
        }
        internal PowerManagerData pmData { get { _pmd = _pmd ?? PO?.data as PowerManagerData ?? new PowerManagerData(null); return _pmd; } }
        private PowerManagerData _pmd;
        private PlacedObject PO;

        public float GetPowerForPoint(Vector2 point)
        {
            var res = pmData.basePowerLevel;
            res += GetGlobalPower();
            foreach (var unit in subs) res += unit.BonusForPoint(point);
            res = Clamp01(res);
            return res;
        }
        public float GetGlobalPower()
        {
            var res = pmData.basePowerLevel;
            foreach (var unit in subs) { res += unit.GlobalBonus(); }
            res = Clamp01(res); 
            return res;
        }

        public void RegisterPowerDevice(IRoomPowerModifier obj)
        {
            subs.Add(obj);
            ValidateDeviceSet();
        }
        private void ValidateDeviceSet()
        {
            for (int i = subs.Count - 1; i >= 0; i--)
            {
                if (subs[i].RemoveOnValidation) subs.RemoveAt(i);
            }
        }

        internal List<IRoomPowerModifier> subs = new List<IRoomPowerModifier>();
        
        public interface IRoomPowerModifier
        {
            bool RemoveOnValidation { get; }
            bool Enabled { get; }
            float BonusForPoint(Vector2 point);
            float GlobalBonus();
        }
    }
}
