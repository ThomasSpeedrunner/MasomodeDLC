using FargowiltasSouls;
using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.Yharon;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.World;
using static Terraria.ModLoader.ModContent;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Items.Potions;
using CalamityMod.CalPlayer;
using MasomodeDLC.Calamity.Buffs;

namespace MasomodeDLC.Calamity
{
	public class MasoCalamityGlobalNPC : GlobalNPC
	{
		public override bool Autoload(ref string name)
		{
			return ModLoader.GetMod("CalamityMod") != null;
		}

		private static readonly Mod Calamity = ModLoader.GetMod("CalamityMod");
		private static readonly Mod FargoSouls = ModLoader.GetMod("FargowiltasSouls");
		public bool[] masoBool = new bool[4];
		public float[] masoFloat = new float[4];
		public int Stop = 0;
		public int Counter = 0;
		public int Counter2 = 0;
		public int CachedDamage;
		public int lastPlayerAttack;

		public override bool InstancePerEntity => true;

		public static int boss = -1;

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			//layers
			int y = spawnInfo.spawnTileY;
			bool cavern = y >= Main.maxTilesY * 0.4f && y <= Main.maxTilesY * 0.8f;
			bool underground = y > Main.worldSurface && y <= Main.maxTilesY * 0.4f;
			bool surface = y < Main.worldSurface && !spawnInfo.sky;
			bool wideUnderground = cavern || underground;
			bool underworld = spawnInfo.player.ZoneUnderworldHeight;
			bool sky = spawnInfo.sky;

			//times
			bool night = !Main.dayTime;
			bool day = Main.dayTime;
			bool rain = Main.raining;

			//biomes
			bool noBiome = MasomodeDLC.NoBiomeNormalSpawn(spawnInfo);
			bool ocean = spawnInfo.player.ZoneBeach;
			bool dungeon = spawnInfo.player.ZoneDungeon;
			bool meteor = spawnInfo.player.ZoneMeteor;
			bool spiderCave = spawnInfo.spiderCave;
			bool mushroom = spawnInfo.player.ZoneGlowshroom;
			bool jungle = spawnInfo.player.ZoneJungle;
			bool granite = spawnInfo.granite;
			bool marble = spawnInfo.marble;
			bool corruption = spawnInfo.player.ZoneCorrupt;
			bool crimson = spawnInfo.player.ZoneCrimson;
			bool snow = spawnInfo.player.ZoneSnow;
			bool hallow = spawnInfo.player.ZoneHoly;
			bool desert = spawnInfo.player.ZoneDesert;

			bool nebulaTower = spawnInfo.player.ZoneTowerNebula;
			bool vortexTower = spawnInfo.player.ZoneTowerVortex;
			bool stardustTower = spawnInfo.player.ZoneTowerStardust;
			bool solarTower = spawnInfo.player.ZoneTowerSolar;

			bool water = spawnInfo.water;

			//events
			bool nearInvasion = spawnInfo.invasion;
			bool goblinArmy = Main.invasionType == 1;
			bool frostLegion = Main.invasionType == 2;
			bool pirates = Main.invasionType == 3;
			bool oldOnesArmy = DD2Event.Ongoing && spawnInfo.player.ZoneOldOneArmy;
			bool frostMoon = surface && night && Main.snowMoon;
			bool pumpkinMoon = surface && night && Main.pumpkinMoon;
			bool solarEclipse = surface && day && Main.eclipse;
			bool martianMadness = Main.invasionType == 4;
			bool lunarEvents = NPC.LunarApocalypseIsUp && (nebulaTower || vortexTower || stardustTower || solarTower);
			bool monsterMadhouse = MMWorld.MMArmy;

			//no work?
			//is lava on screen
			bool nearLava = Collision.LavaCollision(spawnInfo.player.position, spawnInfo.spawnTileX, spawnInfo.spawnTileY);
			bool noInvasion = MasomodeDLC.NoInvasion(spawnInfo);
			bool normalSpawn = !spawnInfo.playerInTown && noInvasion && !oldOnesArmy;

			bool sinisterIcon = spawnInfo.player.GetModPlayer<FargoPlayer>().SinisterIcon;

			if (FargoSoulsWorld.MasochistMode)
			{
				if (night && surface)
				{
					pool[NPCType<BlightedEye>()] = (CalamityWorld.downedPerforator || CalamityWorld.downedHiveMind) ? 0.0215f : 0f;
					pool[NPCType<CalamityEye>()] = CalamityWorld.downedCalamitas ? 0.185f : 0f;
				}
				if (desert)
				{
					pool[NPCType<DriedSeekerHead>()] = CalamityWorld.downedDesertScourge ? 0.037f : 0.0145f;
				}
				if (underworld && !spawnInfo.player.GetModPlayer<CalamityPlayer>().ZoneCalamity)
				{
					pool[NPCType<CalamityEye>()] = CalamityWorld.downedCalamitas ? 0.212f : 0f;
				}
			}
		}

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			float spawnRates = spawnRate;
			float maxSpawnsReal = maxSpawns;
			if (player.HasBuff(BuffType<ForcedPeace>()))
			{
				spawnRates /= .78f;
				maxSpawnsReal *= 0.78f;
			}
			if (player.HasBuff(BuffType<ForcedZen>()))
			{
				spawnRates /= .42f;
				maxSpawnsReal *= 0.42f;
			}
			spawnRate = (int)spawnRates;
			maxSpawns = (int)maxSpawnsReal;
			base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
		}

		public override void SetDefaults(NPC npc)
		{
			FargoSoulsGlobalNPC fargoSoulsGlobalNPC = npc.GetGlobalNPC<FargoSoulsGlobalNPC>();

			if (FargoSoulsWorld.MasochistMode)
			{
				if (npc.type == NPCType<StormlionCharger>())
				{
					npc.lifeMax = 250;
					npc.buffImmune[BuffID.Electrified] = true;
					npc.buffImmune[BuffType<LightningRod>()] = true;
				}
				if (npc.type == NPCType<GhostBell>())
				{
					npc.buffImmune[BuffID.Electrified] = true;
					if (Main.hardMode)
						npc.buffImmune[BuffType<LightningRod>()] = true;
				}
			}
		}

		public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
		{
			if (FargoSoulsWorld.MasochistMode)
			{
				if (npc.type == NPCType<DesertScourgeHead>()
				 || npc.type == NPCType<DesertScourgeHeadSmall>())
				{
					npc.lifeMax = (int)(npc.lifeMax * (1 + MasoDLCWorld.DesertWormBossCount * .025));
					npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.DesertWormBossCount * .0125));
				}

				if (npc.type == NPCType<CrabulonIdle>())
				{
					npc.lifeMax = (int)(npc.lifeMax * (1 + MasoDLCWorld.CrabRaveCount * .025));
					npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.CrabRaveCount * .0125));
				}

				if (npc.type == NPCType<HiveMind>()
				 || npc.type == NPCType<HiveMindP2>())
				{
					npc.lifeMax = (int)(npc.lifeMax * (1 + MasoDLCWorld.HiiHMiiMCount * .025));
					npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.HiiHMiiMCount * .0125));
				}

				if (npc.type == NPCType<PerforatorHive>()
				 || npc.type == NPCType<PerforatorHeadSmall>()
				 || npc.type == NPCType<PerforatorHeadMedium>()
				 || npc.type == NPCType<PerforatorHeadLarge>())
				{
					npc.lifeMax = (int)(npc.lifeMax * (1 + MasoDLCWorld.BloodyWormBossesCount * .025));
					npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.BloodyWormBossesCount * .0125));
				}

				if (npc.type == NPCType<SlimeGodCore>()
				 || npc.type == NPCType<SlimeGodRun>()
				 || npc.type == NPCType<SlimeGodRunSplit>()
				 || npc.type == NPCType<SlimeGodSplit>()
				 || npc.type == NPCType<SlimeGod>())
				{
					npc.lifeMax = (int)(npc.lifeMax * (1 + MasoDLCWorld.BloodyWormBossesCount * .025));
					npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.BloodyWormBossesCount * .0125));
				}
				npc.GetGlobalNPC<MasoCalamityGlobalNPC>().CachedDamage = npc.damage;
			}
		}

		public override bool PreAI(NPC npc)
		{
			Player player = Main.player[npc.target];
			if (FargoSoulsWorld.MasochistMode)
			{
				if (npc.type == NPCType<GhostBell>())
				{
					npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] += 1;
					if (npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] >= 400)
					{
						Aura(npc, 180, BuffID.Electrified, false, DustID.Electric);
						npc.dontTakeDamage = true;
					}
					else
					{
						npc.dontTakeDamage = false;
					}
					if (npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] >= 600)
					{
						npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] = 0;
					}
				}
			}
			return true;
		}

		public override void PostAI(NPC npc)
		{
			if (FargoSoulsWorld.MasochistMode)
			{
				if (npc.damage != CachedDamage)
				{
					if (npc.type == NPCType<DesertScourgeHead>() || npc.type == NPCType<DesertScourgeBody>() || npc.type == NPCType<DesertScourgeTail>()
					 || npc.type == NPCType<DesertScourgeHeadSmall>() || npc.type == NPCType<DesertScourgeBodySmall>() || npc.type == NPCType<DesertScourgeTailSmall>())
					{
						npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.DesertWormBossCount * .0125));
					}

					if (npc.type == NPCType<CrabulonIdle>())
					{
						npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.CrabRaveCount * .0125));
					}

					if (npc.type == NPCType<HiveMind>() || npc.type == NPCType<HiveMindP2>()
					 || npc.type == NPCType<DankCreeper>()
					 || npc.type == NPCType<HiveBlob>() || npc.type == NPCType<HiveBlob2>()
					 || npc.type == NPCType<DarkHeart>())
					{
						npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.HiiHMiiMCount * .0125));
					}

					if (npc.type == NPCType<PerforatorHive>()
					 || npc.type == NPCType<PerforatorHeadSmall>()
					 || npc.type == NPCType<PerforatorHeadMedium>()
					 || npc.type == NPCType<PerforatorHeadLarge>())
					{
						npc.damage = (int)(npc.damage * (1 + MasoDLCWorld.BloodyWormBossesCount * .0125));
					}

					CachedDamage = npc.damage;
				}

				if (npc.type == ModContent.NPCType<StormlionCharger>())
				{
					npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] += 1;
					if (npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] >= 22)
					{
						npc.GetGlobalNPC<MasoCalamityGlobalNPC>().masoFloat[0] = 0;
						Projectile projectile = Projectile.NewProjectileDirect(npc.Center, new Vector2(0, 8).RotatedByRandom(30f), ModContent.ProjectileType<Spark>(), 35, 0f, Main.myPlayer, 0, 0);
						projectile.melee = false;
						projectile.friendly = false;
						projectile.hostile = true;
					}
				}
			}
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			npc.GetGlobalNPC<MasoCalamityGlobalNPC>().lastPlayerAttack = player.whoAmI;
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			npc.GetGlobalNPC<MasoCalamityGlobalNPC>().lastPlayerAttack = projectile.owner;
		}

		public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
		{
			if (FargoSoulsWorld.MasochistMode)
			{
				if (npc.type == NPCType<StormlionCharger>())
				{
					target.AddBuff(BuffType<LightningRod>(), 150);
				}
			}
		}

		public override bool CheckDead(NPC npc)
		{
			if (FargoSoulsWorld.MasochistMode)
			{
			}
			return true;
		}

		public override bool PreNPCLoot(NPC npc)
		{
			Player player = Main.player[npc.GetGlobalNPC<MasoCalamityGlobalNPC>().lastPlayerAttack];
			if (npc.type == NPCType<StormlionCharger>())
			{
				if (player.GetModPlayer<FargoPlayer>().TimsConcoction)
					RunItemDropOnce(npc, ItemType<TriumphPotion>(), Main.rand.Next(1, 3 + 1));
			}

			if (npc.type == NPCType<GhostBell>())
			{
				if (player.GetModPlayer<FargoPlayer>().TimsConcoction)
					RunItemDropOnce(npc, ItemType<TeslaPotion>(), 1, false, 2, 3);
			}

			if (npc.type == NPCType<CalamityEye>())
			{
				RunItemDropWhen(npc, CalamityWorld.downedCalamitas && FargoSoulsWorld.MasochistMode, ItemType<CalamityDust>(), 1, false, 1, 14);
				if (player.GetModPlayer<FargoPlayer>().TimsConcoction)
					RunItemDropWhen(npc, CalamityWorld.downedCalamitas, ItemType<CalamitasBrew>(), Main.rand.Next(2, 3 + 1));
			}
			return base.PreNPCLoot(npc);
		}

		/// <summary>
		/// Drops the specified item if the chance to drop succeeds, defaults to a guaranteed drop.
		/// The return value is whether or not the item was dropped. This can be used to drop additional items if certain ones were dropped (e.g. Stynger causing Stynger Bolts to also drop).
		/// <param name="npc">The entity from which to drop an item.</param>
		/// <param name="itemID">The item to be dropped.</param>
		/// <param name="amount">The amount of the item to drop.</param>
		/// <param name="modifiers">Whether or not the item should have modifiers.</param>
		/// <param name="num">Combines with den to make a fractional (<100%) drop chance; this number is the numerator.</param>
		/// <param name="den">Combines with num to make a fractional (<100%) drop chance; this number is the denominator.</param>
		/// </summary>
		private bool RunItemDrop(NPC npc, int itemID, int amount = 1, bool modifiers = false, int num = 1, int den = 1)
		{
			if (Main.netMode == 1)
				return false;

			if (Main.rand.Next(den) < num)
			{
				Item.NewItem(npc.Hitbox, itemID, amount, false, modifiers ? -1 : 0, false, false);
				return true;
			}
			return false;
		}

		/// <summary>
		/// The same as RunItemDrop, but any further drops for the item will be prevented.
		/// Good for overwriting or reworking loot tables.
		/// </summary>
		/// <param name="npc">The entity from which to drop an item.</param>
		/// <param name="itemID">The item to be dropped.</param>
		/// <param name="amount">The amount of the item to drop.</param>
		/// <param name="modifiers">Whether or not the item should have modifiers.</param>
		/// <param name="num">Combines with den to make a fractional (<100%) drop chance; this number is the numerator.</param>
		/// <param name="den">Combines with num to make a fractional (<100%) drop chance; this number is the denominator.</param>
		/// <returns>Whether the item was dropped or not.</returns>
		private bool RunItemDropOnce(NPC npc, int itemID, int amount = 1, bool modifiers = false, int num = 1, int den = 1)
		{
			bool result = RunItemDrop(npc, itemID, amount, modifiers, num, den);
			NPCLoader.blockLoot.Add(itemID);
			return result;
		}

		private bool RunItemDropWhen(NPC npc, bool condition, int itemID, int amount = 1, bool modifiers = false, int num = 1, int den = 1)
		{
			bool result = false;

			if (condition)
				result = RunItemDropOnce(npc, itemID, amount, modifiers, num, den);
			else
				NPCLoader.blockLoot.Add(itemID);

			return result;
		}

		public override void SpawnNPC(int npc, int tileX, int tileY)
		{

		}

		private void Shoot(NPC npc, int delay, float distance, int speed, int proj, int dmg, float kb, bool recolor = false, bool hostile = false)
		{
			int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
			if (t == -1)
				return;

			Player player = Main.player[t];
			//npc facing player target or if already started attack
			if (player.active && !player.dead && npc.direction == (Math.Sign(player.position.X - npc.position.X)) || Stop > 0)
			{
				//start the pause
				if (delay != 0 && Stop == 0)
				{
					Stop = delay;
				}
				//half way through start attack
				else if (delay == 0 || Stop == delay / 2)
				{
					Vector2 velocity = Vector2.Normalize(player.Center - npc.Center) * speed;
					if (npc.Distance(player.Center) < distance)
						velocity = Vector2.Normalize(player.Center - npc.Center) * speed;
					else //player too far away now, just shoot straight ahead
						velocity = new Vector2(npc.direction * speed, 0);

					int p = Projectile.NewProjectile(npc.Center, velocity, proj, dmg, kb, Main.myPlayer);
					if (p < 1000)
					{
						if (recolor)
							Main.projectile[p].GetGlobalProjectile<FargowiltasSouls.Projectiles.FargoGlobalProjectile>().IsRecolor = true;
						if (hostile)
						{
							Main.projectile[p].friendly = false;
							Main.projectile[p].hostile = true;
						}
					}
					Counter = 0;
				}
			}
		}

		private void Aura(NPC npc, float distance, int buff, bool reverse = false, int dustid = DustID.GoldFlame, bool checkDuration = false)
		{
			//works because buffs are client side anyway :ech:
			Player p = Main.player[Main.myPlayer];
			float range = npc.Distance(p.Center);
			if (reverse ? range > distance && range < 3000f : range < distance)
				p.AddBuff(buff, checkDuration && Main.expertMode && Main.expertDebuffTime > 1 ? 1 : 2);

			for (int i = 0; i < 10; i++)
			{
				Vector2 offset = new Vector2();
				double angle = Main.rand.NextDouble() * 2d * Math.PI;
				offset.X += (float)(Math.Sin(angle) * distance);
				offset.Y += (float)(Math.Cos(angle) * distance);
				Dust dust = Main.dust[Dust.NewDust(
					npc.Center + offset - new Vector2(4, 4), 0, 0,
					dustid, 0, 0, 100, Color.White, 1f
					)];
				dust.velocity = npc.velocity;
				if (Main.rand.Next(3) == 0)
					dust.velocity += Vector2.Normalize(offset) * (reverse ? 5f : -5f);
				dust.noGravity = true;
			}
		}

		public static bool BossIsAlive(ref int bossID, int bossType)
		{
			if (bossID != -1)
			{
				if (Main.npc[bossID].active && Main.npc[bossID].type == bossType)
				{
					return true;
				}
				else
				{
					bossID = -1;
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private bool CalamityNPC(NPC npc)
		{
			if (npc.modNPC != null)
			{
				if (npc.modNPC.mod.Name == "CalamityMod")
				{
					return true;
				}
			}
			return false;
		}
	}
}