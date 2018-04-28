using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace GaussWeapons
{
    public class Projectile_Gauss : Projectile
    {
        // Comps
        private CompExtraDamage compED => GetComp<CompExtraDamage>();
        
        /// <summary>
        /// Impacts a pawn/object or the ground.
        /// </summary>
        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact =
                new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget, this.equipmentDef,
                    this.def);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                var damageAmountBase = this.def.projectile.damageAmountBase;
                var damageDef = this.def.projectile.damageDef;
                var amount = damageAmountBase;
                var y = this.ExactRotation.eulerAngles.y;
                var dinfo = new DamageInfo(damageDef, amount, y, launcher, null, equipmentDef,
                    DamageInfo.SourceCategory.ThingOrUnknown);
                hitThing.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_RangedImpact);
                if (compED != null && hitThing is Pawn pawn && !pawn.Downed && Rand.Value < compED.chanceToProc)
                {
                    GuassImpact(hitThing);
                }
            }
            else
            {
                SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(base.Position, map, false));
                MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    MoteMaker.MakeWaterSplash(this.ExactPosition, map,
                        Mathf.Sqrt((float) this.def.projectile.damageAmountBase) * 1f, 4f);
                }
            }
        }

        private void GuassImpact(Thing hitThing)
        {
            var extraDamageDef = DefDatabase<DamageDef>.GetNamed(compED.damageDef, true);
            var extraDam = new DamageInfo(extraDamageDef,
                compED.damageAmount, this.ExactRotation.eulerAngles.y, this.launcher, null, null);
            var battleLogEntry_RangedImpactExtra =
                new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget, this.equipmentDef,
                    this.def);

            MoteMaker.ThrowMicroSparks(this.destination, Map);
            Find.BattleLog.Add(battleLogEntry_RangedImpactExtra);
            hitThing.TakeDamage(extraDam).InsertIntoLog(battleLogEntry_RangedImpactExtra);
        }
    }
}