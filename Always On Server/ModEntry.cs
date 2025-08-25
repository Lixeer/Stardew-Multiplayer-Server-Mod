using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Always_On_Server.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace Always_On_Server
{
	public class ModEntry : Mod
	{
		private ModConfig Config;

		private int gameTicks;

		private int skipTicks;

		private int gameClockTicks;

		private int numPlayers;

		private bool IsAutomating;

		public int bedX;

		public int bedY;

		public bool clientPaused;

		private string lastInviteCode;

		private bool debug;

		private bool shippingMenuActive;

		private readonly Dictionary<string, int> PreviousFriendships = new Dictionary<string, int>();

		public int connectionsCount = 1;

		private bool eventCommandUsed;

		private bool eggHuntAvailable;

		private int eggHuntCountDown;

		private bool flowerDanceAvailable;

		private int flowerDanceCountDown;

		private bool luauSoupAvailable;

		private int luauSoupCountDown;

		private bool jellyDanceAvailable;

		private int jellyDanceCountDown;

		private bool grangeDisplayAvailable;

		private int grangeDisplayCountDown;

		private bool goldenPumpkinAvailable;

		private int goldenPumpkinCountDown;

		private bool iceFishingAvailable;

		private int iceFishingCountDown;

		private bool winterFeastAvailable;

		private int winterFeastCountDown;

		private int currentTime = Game1.timeOfDay;

		private SDate currentDate = SDate.Now();

		private SDate eggFestival = new SDate(13, "spring");

		private SDate dayAfterEggFestival = new SDate(14, "spring");

		private SDate flowerDance = new SDate(24, "spring");

		private SDate luau = new SDate(11, "summer");

		private SDate danceOfJellies = new SDate(28, "summer");

		private SDate stardewValleyFair = new SDate(16, "fall");

		private SDate spiritsEve = new SDate(27, "fall");

		private SDate festivalOfIce = new SDate(8, "winter");

		private SDate feastOfWinterStar = new SDate(25, "winter");

		private SDate grampasGhost = new SDate(1, "spring", 3);

		private int timeOutTicksForReset;

		private int festivalTicksForReset;

		private int shippingMenuTimeoutTicks;

		private readonly SDate currentDateForReset = SDate.Now();

		private readonly SDate danceOfJelliesForReset = new SDate(28, "summer");

		private readonly SDate spiritsEveForReset = new SDate(27, "fall");

		public override void Entry(IModHelper helper)
		{
			this.Config = base.Helper.ReadConfig<ModConfig>();
			helper.ConsoleCommands.Add("ALOS.server", "Toggles headless 'auto mode' on/off", ToggleAutoMode);
			helper.ConsoleCommands.Add("ALOS.go_to_bed", "TP the bot to bed and sleep", HandlerGoToBed);
			helper.ConsoleCommands.Add("ALOS.debug_server", "Turns debug mode on/off, lets server run when no players are connected", DebugToggle);
			helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			helper.Events.GameLoop.Saving += OnSaving;
			helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
			helper.Events.GameLoop.TimeChanged += OnTimeChanged;
			helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
			helper.Events.Input.ButtonPressed += OnButtonPressed;
			helper.Events.Display.Rendered += OnRendered;
			helper.Events.Specialized.UnvalidatedUpdateTicked += OnUnvalidatedUpdateTick;
		}

        private void HandlerGoToBed(string arg1, string[] arg2)
        {
            this.GoToBed();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			if (Game1.IsServer)
			{
				ModData data = base.Helper.Data.ReadJsonFile<ModData>("data/" + Constants.SaveFolderName + ".json") ?? new ModData();
				data.FarmingLevel = Game1.player.FarmingLevel;
				data.MiningLevel = Game1.player.MiningLevel;
				data.ForagingLevel = Game1.player.ForagingLevel;
				data.FishingLevel = Game1.player.FishingLevel;
				data.CombatLevel = Game1.player.CombatLevel;
				base.Helper.Data.WriteJsonFile("data/" + Constants.SaveFolderName + ".json", data);
				Game1.player.farmingLevel.Value = 10;
				Game1.player.miningLevel.Value = 10;
				Game1.player.foragingLevel.Value = 10;
				Game1.player.fishingLevel.Value = 10;
				Game1.player.combatLevel.Value = 10;
				this.IsAutomating = true;
				Game1.chatBox.addInfoMessage("The host is in automatic mode!");
				base.Monitor.Log("Auto Mode On!", LogLevel.Info);
			}
		}

		private void DebugToggle(string command, string[] args)
		{
			if (Context.IsWorldReady)
			{
				this.debug = !this.debug;
				base.Monitor.Log("Server Debug " + (this.debug ? "On" : "Off"), LogLevel.Info);
			}
		}

		public static void DrawTextBox(int x, int y, SpriteFont font, string message, int align = 0, float colorIntensity = 1f)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			SpriteBatch spriteBatch = Game1.spriteBatch;
			int width = (int)font.MeasureString(message).X + 32;
			int num = (int)font.MeasureString(message).Y + 21;
			switch (align)
			{
			case 0:
				IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, num + 4, Color.White * colorIntensity);
				Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2((float)(x + 16), (float)(y + 16)), Game1.textColor);
				break;
			case 1:
				IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x - width / 2, y, width, num + 4, Color.White * colorIntensity);
				Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2((float)(x + 16 - width / 2), (float)(y + 16)), Game1.textColor);
				break;
			case 2:
				IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x - width, y, width, num + 4, Color.White * colorIntensity);
				Utility.drawTextWithShadow(spriteBatch, message, font, new Vector2((float)(x + 16 - width), (float)(y + 16)), Game1.textColor);
				break;
			}
		}

		private void OnRendered(object sender, RenderedEventArgs e)
		{
			if (Game1.options.enableServer && this.IsAutomating)
			{
				int connections = Game1.server.connectionsCount;
				ModEntry.DrawTextBox(5, 100, Game1.dialogueFont, "Auto Mode On");
				ModEntry.DrawTextBox(5, 180, Game1.dialogueFont, $"Press {this.Config.serverHotKey} On/Off");
				float profitMargin = this.Config.profitmargin;
				ModEntry.DrawTextBox(5, 260, Game1.dialogueFont, $"Profit Margin: {profitMargin}%");
				ModEntry.DrawTextBox(5, 340, Game1.dialogueFont, $"{connections} Players Online");
				if (this.lastInviteCode != null)
				{
					ModEntry.DrawTextBox(5, 420, Game1.dialogueFont, "Invite Code: " + this.lastInviteCode);
				}
			}
		}

		private void ToggleAutoMode(string command, string[] args)
		{
			if (Context.IsWorldReady)
			{
				if (!this.IsAutomating)
				{
					base.Helper.ReadConfig<ModConfig>();
					this.IsAutomating = true;
					base.Monitor.Log("Auto mode on!", LogLevel.Info);
					Game1.chatBox.addInfoMessage("The host is in automatic mode!");
					Game1.displayHUD = true;
					Game1.addHUDMessage(new HUDMessage("Auto Mode On!"));
					Game1.options.pauseWhenOutOfFocus = false;
					ModData data = base.Helper.Data.ReadJsonFile<ModData>("data/" + Constants.SaveFolderName + ".json") ?? new ModData();
					data.FarmingLevel = Game1.player.FarmingLevel;
					data.MiningLevel = Game1.player.MiningLevel;
					data.ForagingLevel = Game1.player.ForagingLevel;
					data.FishingLevel = Game1.player.FishingLevel;
					data.CombatLevel = Game1.player.CombatLevel;
					base.Helper.Data.WriteJsonFile("data/" + Constants.SaveFolderName + ".json", data);
					Game1.player.farmingLevel.Value = 10;
					Game1.player.miningLevel.Value = 10;
					Game1.player.foragingLevel.Value = 10;
					Game1.player.fishingLevel.Value = 10;
					Game1.player.combatLevel.Value = 10;
				}
				else
				{
					this.IsAutomating = false;
					base.Monitor.Log("Auto mode off!", LogLevel.Info);
					Game1.chatBox.addInfoMessage("The host has returned!");
					Game1.displayHUD = true;
					Game1.addHUDMessage(new HUDMessage("Auto Mode Off!"));
					ModData data2 = base.Helper.Data.ReadJsonFile<ModData>("data/" + Constants.SaveFolderName + ".json") ?? new ModData();
					Game1.player.farmingLevel.Value = data2.FarmingLevel;
					Game1.player.miningLevel.Value = data2.MiningLevel;
					Game1.player.foragingLevel.Value = data2.ForagingLevel;
					Game1.player.fishingLevel.Value = data2.FishingLevel;
					Game1.player.combatLevel.Value = data2.CombatLevel;
				}
			}
		}

		private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (Context.IsWorldReady && e.Button == this.Config.serverHotKey)
			{
				if (!this.IsAutomating)
				{
					base.Helper.ReadConfig<ModConfig>();
					this.IsAutomating = true;
					base.Monitor.Log("Auto mode on!", LogLevel.Info);
					Game1.chatBox.addInfoMessage("The host is in automatic mode!");
					Game1.displayHUD = true;
					Game1.addHUDMessage(new HUDMessage("Auto Mode On!"));
					Game1.options.pauseWhenOutOfFocus = false;
					ModData data = base.Helper.Data.ReadJsonFile<ModData>("data/" + Constants.SaveFolderName + ".json") ?? new ModData();
					data.FarmingLevel = Game1.player.FarmingLevel;
					data.MiningLevel = Game1.player.MiningLevel;
					data.ForagingLevel = Game1.player.ForagingLevel;
					data.FishingLevel = Game1.player.FishingLevel;
					data.CombatLevel = Game1.player.CombatLevel;
					base.Helper.Data.WriteJsonFile("data/" + Constants.SaveFolderName + ".json", data);
					Game1.player.farmingLevel.Value = 10;
					Game1.player.miningLevel.Value = 10;
					Game1.player.foragingLevel.Value = 10;
					Game1.player.fishingLevel.Value = 10;
					Game1.player.combatLevel.Value = 10;
				}
				else
				{
					this.IsAutomating = false;
					base.Monitor.Log("Auto mode off!", LogLevel.Info);
					Game1.chatBox.addInfoMessage("The host has returned!");
					Game1.displayHUD = true;
					Game1.addHUDMessage(new HUDMessage("Auto Mode Off!"));
					ModData data2 = base.Helper.Data.ReadJsonFile<ModData>("data/" + Constants.SaveFolderName + ".json") ?? new ModData();
					Game1.player.farmingLevel.Value = data2.FarmingLevel;
					Game1.player.miningLevel.Value = data2.MiningLevel;
					Game1.player.foragingLevel.Value = data2.ForagingLevel;
					Game1.player.fishingLevel.Value = data2.FishingLevel;
					Game1.player.combatLevel.Value = data2.CombatLevel;
				}
				if (Game1.player.currentLocation is FarmHouse)
				{
					Game1.warpFarmer("Farm", 64, 15, false);
					return;
				}
				this.GetBedCoordinates();
				Game1.warpFarmer("Farmhouse", this.bedX, this.bedY, false);
			}
		}

		private void FestivalsToggle()
		{
			bool festivalsOn = this.Config.festivalsOn;
		}

		private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			if (this.IsAutomating)
			{
				this.PauseIfNobodyPresent();
			}
			else
			{
				Game1.netWorldState.Value.IsPaused = false;
			}
			if (this.IsAutomating && this.Config.clientsCanPause)
			{
				List<ChatMessage> messages = base.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages").GetValue();
				if (messages.Count > 0)
				{
					List<ChatSnippet> messagetoconvert = messages[messages.Count - 1].message;
					string actualmessage = ChatMessage.makeMessagePlaintext(messagetoconvert, true);
					string lastFragment = actualmessage.Split(' ')[1];
					if (lastFragment != null && lastFragment == "!pause")
					{
						Game1.netWorldState.Value.IsPaused = true;
						this.clientPaused = true;
						this.SendChatMessage("Game Paused");
					}
					if (lastFragment != null && lastFragment == "!unpause")
					{
						Game1.netWorldState.Value.IsPaused = false;
						this.clientPaused = false;
						this.SendChatMessage("Game UnPaused");
					}
				}
			}
			if (Game1.options.enableServer && Game1.server != null)
			{
				string inviteCode = Game1.server.getInviteCode();
				if (inviteCode != this.lastInviteCode)
				{
					if (this.Config.copyInviteCodeToClipboard)
					{
						DesktopClipboard.SetText("Invite Code: " + Game1.server.getInviteCode());
					}
					try
					{
						File.WriteAllText(Path.Combine(base.Helper.DirectoryPath, "InviteCode.txt"), inviteCode);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception: " + ex.Message);
					}
					this.lastInviteCode = inviteCode;
				}
			}
			if (Game1.options.enableServer && Game1.server != null && this.connectionsCount != Game1.server.connectionsCount)
			{
				this.connectionsCount = Game1.server.connectionsCount;
				try
				{
					File.WriteAllText(Path.Combine(base.Helper.DirectoryPath, "ConnectionsCount.txt"), this.connectionsCount.ToString());
				}
				catch (Exception ex2)
				{
					Console.WriteLine("Exception: " + ex2.Message);
				}
			}
			if (this.IsAutomating)
			{
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
				{
					Game1.activeClickableMenu.receiveLeftClick(10, 10);
				}
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.skippable)
				{
					this.skipTicks++;
					if (this.skipTicks >= 3)
					{
						Game1.CurrentEvent.skipEvent();
						this.skipTicks = 0;
					}
				}
			}
			if (this.IsAutomating)
			{
				if (this.PreviousFriendships.Any())
				{
					foreach (string key in Game1.player.friendshipData.Keys)
					{
						Friendship friendship = Game1.player.friendshipData[key];
						int oldPoints;
						if (this.PreviousFriendships.TryGetValue(key, out oldPoints) && oldPoints > friendship.Points)
						{
							friendship.Points = oldPoints;
						}
					}
				}
				this.PreviousFriendships.Clear();
				foreach (KeyValuePair<string, NetRef<Friendship>> pair in Game1.player.friendshipData.FieldDict)
				{
					this.PreviousFriendships[pair.Key] = pair.Value.Value.Points;
				}
			}
			if (this.IsAutomating)
			{
				if (this.eggHuntAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.eggHuntCountDown = this.Config.eggHuntCountDownConfig;
						this.eventCommandUsed = false;
					}
					this.eggHuntCountDown++;
					float chatEgg = (float)this.Config.eggHuntCountDownConfig / 60f;
					if (this.eggHuntCountDown == 1)
					{
						this.SendChatMessage($"The Egg Hunt will begin in {chatEgg:0.#} minutes.");
					}
					if (this.eggHuntCountDown == this.Config.eggHuntCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.eggHuntCountDown >= this.Config.eggHuntCountDownConfig + 5)
					{
						IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
						this.festivalTicksForReset++;
						if (this.festivalTicksForReset >= this.Config.eggFestivalTimeOut + 180)
						{
							Game1.options.setServerMode("offline");
						}
					}
				}
				if (this.flowerDanceAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.flowerDanceCountDown = this.Config.flowerDanceCountDownConfig;
						this.eventCommandUsed = false;
					}
					this.flowerDanceCountDown++;
					float chatFlower = (float)this.Config.flowerDanceCountDownConfig / 60f;
					if (this.flowerDanceCountDown == 1)
					{
						this.SendChatMessage($"The Flower Dance will begin in {chatFlower:0.#} minutes.");
					}
					if (this.flowerDanceCountDown == this.Config.flowerDanceCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.flowerDanceCountDown >= this.Config.flowerDanceCountDownConfig + 5)
					{
						IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
						this.festivalTicksForReset++;
						if (this.festivalTicksForReset >= this.Config.flowerDanceTimeOut + 90)
						{
							Game1.options.setServerMode("offline");
						}
					}
				}
				if (this.luauSoupAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.luauSoupCountDown = this.Config.luauSoupCountDownConfig;
						StardewValley.Object item = new StardewValley.Object("268", 1, false, -1, 3);
						base.Helper.Reflection.GetMethod(new Event(), "addItemToLuauSoup").Invoke(item, Game1.player);
						this.eventCommandUsed = false;
					}
					this.luauSoupCountDown++;
					float chatSoup = (float)this.Config.luauSoupCountDownConfig / 60f;
					if (this.luauSoupCountDown == 1)
					{
						this.SendChatMessage($"The Soup Tasting will begin in {chatSoup:0.#} minutes.");
						StardewValley.Object item2 = new StardewValley.Object("268", 1, false, -1, 3);
						base.Helper.Reflection.GetMethod(new Event(), "addItemToLuauSoup").Invoke(item2, Game1.player);
					}
					if (this.luauSoupCountDown == this.Config.luauSoupCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.luauSoupCountDown >= this.Config.luauSoupCountDownConfig + 5)
					{
						IClickableMenu activeClickableMenu3 = Game1.activeClickableMenu;
						this.festivalTicksForReset++;
						if (this.festivalTicksForReset >= this.Config.luauTimeOut + 80)
						{
							Game1.options.setServerMode("offline");
						}
					}
				}
				if (this.jellyDanceAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.jellyDanceCountDown = this.Config.jellyDanceCountDownConfig;
						this.eventCommandUsed = false;
					}
					this.jellyDanceCountDown++;
					float chatJelly = (float)this.Config.jellyDanceCountDownConfig / 60f;
					if (this.jellyDanceCountDown == 1)
					{
						this.SendChatMessage($"The Dance of the Moonlight Jellies will begin in {chatJelly:0.#} minutes.");
					}
					if (this.jellyDanceCountDown == this.Config.jellyDanceCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.jellyDanceCountDown >= this.Config.jellyDanceCountDownConfig + 5)
					{
						IClickableMenu activeClickableMenu4 = Game1.activeClickableMenu;
						this.festivalTicksForReset++;
						if (this.festivalTicksForReset >= this.Config.danceOfJelliesTimeOut + 180)
						{
							Game1.options.setServerMode("offline");
						}
					}
				}
				if (this.grangeDisplayAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.grangeDisplayCountDown = this.Config.grangeDisplayCountDownConfig;
						this.eventCommandUsed = false;
					}
					this.grangeDisplayCountDown++;
					this.festivalTicksForReset++;
					if (this.festivalTicksForReset == this.Config.fairTimeOut - 120)
					{
						this.SendChatMessage("2 minutes to the exit or");
						this.SendChatMessage("everyone will be kicked.");
					}
					if (this.festivalTicksForReset >= this.Config.fairTimeOut)
					{
						Game1.options.setServerMode("offline");
					}
					float chatGrange = (float)this.Config.grangeDisplayCountDownConfig / 60f;
					if (this.grangeDisplayCountDown == 1)
					{
						this.SendChatMessage($"The Grange Judging will begin in {chatGrange:0.#} minutes.");
					}
					if (this.grangeDisplayCountDown == this.Config.grangeDisplayCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.grangeDisplayCountDown == this.Config.grangeDisplayCountDownConfig + 5)
					{
						this.LeaveFestival();
					}
				}
				if (this.goldenPumpkinAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					this.goldenPumpkinCountDown++;
					this.festivalTicksForReset++;
					if (this.festivalTicksForReset == this.Config.spiritsEveTimeOut - 120)
					{
						this.SendChatMessage("2 minutes to the exit or");
						this.SendChatMessage("everyone will be kicked.");
					}
					if (this.festivalTicksForReset >= this.Config.spiritsEveTimeOut)
					{
						Game1.options.setServerMode("offline");
					}
					if (this.goldenPumpkinCountDown == 10)
					{
						this.LeaveFestival();
					}
				}
				if (this.iceFishingAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.eventCommandUsed)
					{
						this.iceFishingCountDown = this.Config.iceFishingCountDownConfig;
						this.eventCommandUsed = false;
					}
					this.iceFishingCountDown++;
					float chatIceFish = (float)this.Config.iceFishingCountDownConfig / 60f;
					if (this.iceFishingCountDown == 1)
					{
						this.SendChatMessage($"The Ice Fishing Contest will begin in {chatIceFish:0.#} minutes.");
					}
					if (this.iceFishingCountDown == this.Config.iceFishingCountDownConfig + 1)
					{
						base.Helper.Reflection.GetMethod(Game1.CurrentEvent, "answerDialogueQuestion").Invoke(Game1.getCharacterFromName("Lewis"), "yes");
					}
					if (this.iceFishingCountDown >= this.Config.iceFishingCountDownConfig + 5)
					{
						IClickableMenu activeClickableMenu5 = Game1.activeClickableMenu;
						this.festivalTicksForReset++;
						if (this.festivalTicksForReset >= this.Config.festivalOfIceTimeOut + 180)
						{
							Game1.options.setServerMode("offline");
						}
					}
				}
				if (this.winterFeastAvailable && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					this.winterFeastCountDown++;
					this.festivalTicksForReset++;
					if (this.festivalTicksForReset == this.Config.winterStarTimeOut - 120)
					{
						this.SendChatMessage("2 minutes to the exit or");
						this.SendChatMessage("everyone will be kicked.");
					}
					if (this.festivalTicksForReset >= this.Config.winterStarTimeOut)
					{
						Game1.options.setServerMode("offline");
					}
					if (this.winterFeastCountDown == 10)
					{
						this.LeaveFestival();
					}
				}
			}
			if (this.IsAutomating && Game1.activeClickableMenu is LevelUpMenu)
			{
				this.shippingMenuActive = true;
                base.Monitor.Log("Skipping shipping menu");
                base.Helper.Reflection.GetMethod(Game1.activeClickableMenu, "okButtonClicked").Invoke();
			}
		}

		private void PauseIfNobodyPresent()
		{
			this.gameTicks++;
			if (this.gameTicks >= 3)
			{
				this.numPlayers = Game1.otherFarmers.Count;
				if (this.numPlayers >= 1 || this.debug)
				{
					if (this.clientPaused)
					{
						Game1.netWorldState.Value.IsPaused = true;
					}
					else
					{
						Game1.netWorldState.Value.IsPaused = false;
					}
				}
				else if (this.numPlayers <= 0 && Game1.timeOfDay >= 610 && Game1.timeOfDay <= 2500 && this.currentDate != this.eggFestival && this.currentDate != this.flowerDance && this.currentDate != this.luau && this.currentDate != this.danceOfJellies && this.currentDate != this.stardewValleyFair && this.currentDate != this.spiritsEve && this.currentDate != this.festivalOfIce && this.currentDate != this.feastOfWinterStar)
				{
					Game1.netWorldState.Value.IsPaused = true;
				}
				this.gameTicks = 0;
			}
			if (!Context.IsWorldReady || !this.IsAutomating)
			{
				return;
			}
			List<ChatMessage> messages = base.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages").GetValue();
			if (messages.Count <= 0)
			{
				return;
			}
			List<ChatSnippet> messagetoconvert = messages[messages.Count - 1].message;
			string actualmessage = ChatMessage.makeMessagePlaintext(messagetoconvert, true);
			string lastFragment = actualmessage.Split(' ')[1];
			if (lastFragment == null)
			{
				return;
			}
			if (lastFragment == "!sleep")
			{
				if (this.currentTime >= this.Config.timeOfDayToSleep)
				{
					this.GoToBed();
					this.SendChatMessage("Trying to go to bed.");
				}
				else
				{
					this.SendChatMessage("It's too early.");
					this.SendChatMessage($"Try after {this.Config.timeOfDayToSleep}.");
				}
			}
			if (lastFragment == "!festival")
			{
				this.SendChatMessage("Trying to go to Festival.");
				if (this.currentDate == this.eggFestival)
				{
					this.EggFestival();
				}
				else if (this.currentDate == this.flowerDance)
				{
					this.FlowerDance();
				}
				else if (this.currentDate == this.luau)
				{
					this.Luau();
				}
				else if (this.currentDate == this.danceOfJellies)
				{
					this.DanceOfTheMoonlightJellies();
				}
				else if (this.currentDate == this.stardewValleyFair)
				{
					this.StardewValleyFair();
				}
				else if (this.currentDate == this.spiritsEve)
				{
					this.SpiritsEve();
				}
				else if (this.currentDate == this.festivalOfIce)
				{
					this.FestivalOfIce();
				}
				else if (this.currentDate == this.feastOfWinterStar)
				{
					this.FeastOfWinterStar();
				}
				else
				{
					this.SendChatMessage("Festival Not Ready.");
				}
			}
			if (lastFragment == "!event")
			{
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					if (this.currentDate == this.eggFestival)
					{
						this.eventCommandUsed = true;
						this.eggHuntAvailable = true;
					}
					else if (this.currentDate == this.flowerDance)
					{
						this.eventCommandUsed = true;
						this.flowerDanceAvailable = true;
					}
					else if (this.currentDate == this.luau)
					{
						this.eventCommandUsed = true;
						this.luauSoupAvailable = true;
					}
					else if (this.currentDate == this.danceOfJellies)
					{
						this.eventCommandUsed = true;
						this.jellyDanceAvailable = true;
					}
					else if (this.currentDate == this.stardewValleyFair)
					{
						this.eventCommandUsed = true;
						this.grangeDisplayAvailable = true;
					}
					else if (this.currentDate == this.spiritsEve)
					{
						this.eventCommandUsed = true;
						this.goldenPumpkinAvailable = true;
					}
					else if (this.currentDate == this.festivalOfIce)
					{
						this.eventCommandUsed = true;
						this.iceFishingAvailable = true;
					}
					else if (this.currentDate == this.feastOfWinterStar)
					{
						this.eventCommandUsed = true;
						this.winterFeastAvailable = true;
					}
				}
				else
				{
					this.SendChatMessage("I'm not at a Festival.");
				}
			}
			if (lastFragment == "!leave")
			{
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
				{
					this.SendChatMessage("Trying to leave Festival");
					this.LeaveFestival();
				}
				else
				{
					this.SendChatMessage("I'm not at a Festival.");
				}
			}
			if (lastFragment == "!unstick")
			{
				if (Game1.player.currentLocation is FarmHouse)
				{
					this.SendChatMessage("Warping to Farm.");
					Game1.warpFarmer("Farm", 64, 15, false);
				}
				else
				{
					this.SendChatMessage("Warping inside house.");
					this.GetBedCoordinates();
					Game1.warpFarmer("Farmhouse", this.bedX, this.bedY, false);
				}
			}
		}

		private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (this.Config.lockPlayerChests)
			{
				foreach (Farmer farmer in Game1.getOnlineFarmers())
				{
					if (farmer.IsMainPlayer)
					{
						continue;
					}
					FarmHouse house = farmer.currentLocation as FarmHouse;
					if (house == null || farmer == house.owner)
					{
						continue;
					}
					Cabin cabin = house as Cabin;
					if (cabin != null)
					{
						NetMutex inventoryMutex = base.Helper.Reflection.GetField<NetMutex>(cabin, "inventoryMutex").GetValue();
						inventoryMutex.RequestLock();
					}
					house.fridge.Value.mutex.RequestLock();
					foreach (Chest chest in house.objects.Values.OfType<Chest>())
					{
						chest.mutex.RequestLock();
					}
				}
			}
			if (!this.IsAutomating)
			{
				return;
			}
			if (!Game1.player.hasPet())
			{
				base.Helper.Reflection.GetMethod(new Event(), "namePet").Invoke(this.Config.petname.Substring(0));
			}
			if (Game1.player.hasPet())
			{
				Pet pet = Game1.getCharacterFromName(Game1.player.getPetName()) as Pet;
				if (pet != null)
				{
					pet.Name = this.Config.petname.Substring(0);
					pet.displayName = this.Config.petname.Substring(0);
				}
			}
			if (!Game1.player.eventsSeen.Contains("65"))
			{
				Game1.player.eventsSeen.Add("65");
				if (this.Config.farmcavechoicemushrooms)
				{
					Game1.MasterPlayer.caveChoice.Value = 2;
					(Game1.getLocationFromName("FarmCave") as FarmCave).setUpMushroomHouse();
				}
				else
				{
					Game1.MasterPlayer.caveChoice.Value = 1;
				}
			}
			if (!Game1.player.eventsSeen.Contains("611439"))
			{
				Game1.player.eventsSeen.Add("611439");
				Game1.MasterPlayer.mailReceived.Add("ccDoorUnlock");
			}
			if (this.Config.upgradeHouse != 0 && Game1.player.HouseUpgradeLevel != this.Config.upgradeHouse)
			{
				Game1.player.HouseUpgradeLevel = this.Config.upgradeHouse;
			}
			this.IsAutomating &= !(Game1.activeClickableMenu is TitleMenu);
		}

		public void OnTimeChanged(object sender, TimeChangedEventArgs e)
		{
			this.currentTime = Game1.timeOfDay;
			this.currentDate = SDate.Now();
			this.eggFestival = new SDate(13, "spring");
			this.dayAfterEggFestival = new SDate(14, "spring");
			this.flowerDance = new SDate(24, "spring");
			this.luau = new SDate(11, "summer");
			this.danceOfJellies = new SDate(28, "summer");
			this.stardewValleyFair = new SDate(16, "fall");
			this.spiritsEve = new SDate(27, "fall");
			this.festivalOfIce = new SDate(8, "winter");
			this.feastOfWinterStar = new SDate(25, "winter");
			this.grampasGhost = new SDate(1, "spring", 3);
			if (this.IsAutomating)
			{
				this.gameClockTicks++;
				if (this.gameClockTicks >= 3)
				{
					if (this.currentDate == this.eggFestival && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Egg Festival Today!");
							this.SendChatMessage("I will not be in bed until after 2:00 P.M.");
						}
						this.EggFestival();
					}
					else if (this.currentDate == this.flowerDance && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Flower Dance Today.");
							this.SendChatMessage("I will not be in bed until after 2:00 P.M.");
						}
						this.FlowerDance();
					}
					else if (this.currentDate == this.luau && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Luau Today!");
							this.SendChatMessage("I will not be in bed until after 2:00 P.M.");
						}
						this.Luau();
					}
					else if (this.currentDate == this.danceOfJellies && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Dance of the Moonlight Jellies Tonight!");
							this.SendChatMessage("I will not be in bed until after 12:00 A.M.");
						}
						this.DanceOfTheMoonlightJellies();
					}
					else if (this.currentDate == this.stardewValleyFair && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Stardew Valley Fair Today!");
							this.SendChatMessage("I will not be in bed until after 3:00 P.M.");
						}
						this.StardewValleyFair();
					}
					else if (this.currentDate == this.spiritsEve && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Spirit's Eve Tonight!");
							this.SendChatMessage("I will not be in bed until after 12:00 A.M.");
						}
						this.SpiritsEve();
					}
					else if (this.currentDate == this.festivalOfIce && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Festival of Ice Today!");
							this.SendChatMessage("I will not be in bed until after 2:00 P.M.");
						}
						this.FestivalOfIce();
					}
					else if (this.currentDate == this.feastOfWinterStar && (this.numPlayers >= 1 || this.debug))
					{
						this.FestivalsToggle();
						if (this.currentTime >= 600 && this.currentTime <= 630)
						{
							this.SendChatMessage("Feast of the Winter Star Today!");
							this.SendChatMessage("I will not be in bed until after 2:00 P.M.");
						}
						this.FeastOfWinterStar();
					}
					else if (this.currentTime >= this.Config.timeOfDayToSleep && (this.numPlayers >= 1 || this.debug))
					{
						this.GoToBed();
					}
					this.gameClockTicks = 0;
				}
			}
			if (!this.IsAutomating || !(this.currentDate != this.grampasGhost) || !(this.currentDate != this.eggFestival) || !(this.currentDate != this.flowerDance) || !(this.currentDate != this.luau) || !(this.currentDate != this.danceOfJellies) || !(this.currentDate != this.stardewValleyFair) || !(this.currentDate != this.spiritsEve) || !(this.currentDate != this.festivalOfIce) || !(this.currentDate != this.feastOfWinterStar))
			{
				return;
			}
			if (this.currentTime == 620)
			{
				for (int i = 0; i < 10; i++)
				{
					base.Helper.Reflection.GetMethod(Game1.currentLocation, "mailbox").Invoke();
				}
			}
			if (this.currentTime == 630)
			{
				if (!Game1.player.hasRustyKey)
				{
					int items = Game1.netWorldState.Value.MuseumPieces.Length;
					if (items >= 60)
					{
						Game1.player.eventsSeen.Add("295672");
						Game1.player.eventsSeen.Add("66");
						Game1.player.hasRustyKey = true;
					}
				}
				if (this.Config.communitycenterrun && !Game1.player.eventsSeen.Contains("191393") && Game1.player.mailReceived.Contains("ccCraftsRoom") && Game1.player.mailReceived.Contains("ccVault") && Game1.player.mailReceived.Contains("ccFishTank") && Game1.player.mailReceived.Contains("ccBoilerRoom") && Game1.player.mailReceived.Contains("ccPantry") && Game1.player.mailReceived.Contains("ccBulletin"))
				{
					CommunityCenter locationFromName = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
					for (int index = 0; index < locationFromName.areasComplete.Count; index++)
					{
						locationFromName.areasComplete[index] = true;
					}
					Game1.player.eventsSeen.Add("191393");
				}
				if (!this.Config.communitycenterrun)
				{
					if (Game1.player.Money >= 10000 && !Game1.player.mailReceived.Contains("JojaMember"))
					{
						Game1.player.Money -= 5000;
						Game1.player.mailReceived.Add("JojaMember");
						this.SendChatMessage("Buying Joja Membership");
					}
					if (Game1.player.Money >= 30000 && !Game1.player.mailReceived.Contains("jojaBoilerRoom"))
					{
						Game1.player.Money -= 15000;
						Game1.player.mailReceived.Add("ccBoilerRoom");
						Game1.player.mailReceived.Add("jojaBoilerRoom");
						this.SendChatMessage("Buying Joja Minecarts");
					}
					if (Game1.player.Money >= 40000 && !Game1.player.mailReceived.Contains("jojaFishTank"))
					{
						Game1.player.Money -= 20000;
						Game1.player.mailReceived.Add("ccFishTank");
						Game1.player.mailReceived.Add("jojaFishTank");
						this.SendChatMessage("Buying Joja Panning");
					}
					if (Game1.player.Money >= 50000 && !Game1.player.mailReceived.Contains("jojaCraftsRoom"))
					{
						Game1.player.Money -= 25000;
						Game1.player.mailReceived.Add("ccCraftsRoom");
						Game1.player.mailReceived.Add("jojaCraftsRoom");
						this.SendChatMessage("Buying Joja Bridge");
					}
					if (Game1.player.Money >= 70000 && !Game1.player.mailReceived.Contains("jojaPantry"))
					{
						Game1.player.Money -= 35000;
						Game1.player.mailReceived.Add("ccPantry");
						Game1.player.mailReceived.Add("jojaPantry");
						this.SendChatMessage("Buying Joja Greenhouse");
					}
					if (Game1.player.Money >= 80000 && !Game1.player.mailReceived.Contains("jojaVault"))
					{
						Game1.player.Money -= 40000;
						Game1.player.mailReceived.Add("ccVault");
						Game1.player.mailReceived.Add("jojaVault");
						this.SendChatMessage("Buying Joja Bus");
						Game1.player.eventsSeen.Add("502261");
					}
				}
			}
			if (this.currentTime == 640)
			{
				Game1.warpFarmer("Farm", 64, 15, false);
			}
			if (this.currentTime == 900 && !Game1.player.eventsSeen.Contains("739330"))
			{
				Game1.player.increaseBackpackSize(1);
				Game1.warpFarmer("Beach", 1, 20, 1);
			}
		}

		public void EggFestival()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Town", 1, 20, 1);
				});
				this.eggHuntAvailable = true;
			}
			else if (this.currentTime >= 1410)
			{
				this.eggHuntAvailable = false;
				Game1.options.setServerMode("online");
				this.eggHuntCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void FlowerDance()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Forest", 1, 20, 1);
				});
				this.flowerDanceAvailable = true;
			}
			else if (this.currentTime >= 1410 && this.currentTime >= this.Config.timeOfDayToSleep)
			{
				this.flowerDanceAvailable = false;
				Game1.options.setServerMode("online");
				this.flowerDanceCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void Luau()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Beach", 1, 20, 1);
				});
				this.luauSoupAvailable = true;
			}
			else if (this.currentTime >= 1410)
			{
				this.luauSoupAvailable = false;
				Game1.options.setServerMode("online");
				this.luauSoupCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void DanceOfTheMoonlightJellies()
		{
			if (this.currentTime >= 2200 && this.currentTime <= 2400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Beach", 1, 20, 1);
				});
				this.jellyDanceAvailable = true;
			}
			else if (this.currentTime >= 2410)
			{
				this.jellyDanceAvailable = false;
				Game1.options.setServerMode("online");
				this.jellyDanceCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void StardewValleyFair()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1500)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Town", 1, 20, 1);
				});
				this.grangeDisplayAvailable = true;
			}
			else if (this.currentTime >= 1510)
			{
				Game1.displayHUD = true;
				this.grangeDisplayAvailable = false;
				Game1.options.setServerMode("online");
				this.grangeDisplayCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void SpiritsEve()
		{
			if (this.currentTime >= 2200 && this.currentTime <= 2350)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Town", 1, 20, 1);
				});
				this.goldenPumpkinAvailable = true;
			}
			else if (this.currentTime >= 2400)
			{
				Game1.displayHUD = true;
				this.goldenPumpkinAvailable = false;
				Game1.options.setServerMode("online");
				this.goldenPumpkinCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void FestivalOfIce()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Forest", 1, 20, 1);
				});
				this.iceFishingAvailable = true;
			}
			else if (this.currentTime >= 1410)
			{
				this.iceFishingAvailable = false;
				Game1.options.setServerMode("online");
				this.iceFishingCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		public void FeastOfWinterStar()
		{
			if (this.currentTime >= 900 && this.currentTime <= 1400)
			{
				Game1.netReady.SetLocalReady("festivalStart", true);
				Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate
				{
					Game1.exitActiveMenu();
					Game1.warpFarmer("Town", 1, 20, 1);
				});
				this.winterFeastAvailable = true;
			}
			else if (this.currentTime >= 1410)
			{
				this.winterFeastAvailable = false;
				Game1.options.setServerMode("online");
				this.winterFeastCountDown = 0;
				this.festivalTicksForReset = 0;
				this.GoToBed();
			}
		}

		private void GetBedCoordinates()
		{
			switch (Game1.player.HouseUpgradeLevel)
			{
			case 0:
				this.bedX = 9;
				this.bedY = 9;
				break;
			case 1:
				this.bedX = 21;
				this.bedY = 4;
				break;
			default:
				this.bedX = 27;
				this.bedY = 13;
				break;
			}
		}

		private void GoToBed()
		{
			this.GetBedCoordinates();
			Game1.warpFarmer("Farmhouse", this.bedX, this.bedY, false);
			base.Helper.Reflection.GetMethod(Game1.currentLocation, "startSleep").Invoke();
			Game1.displayHUD = true;
		}

		private void OnSaving(object sender, SavingEventArgs e)
		{
			if (this.IsAutomating)
			{
				base.Monitor.Log("This is the Shipping Menu");
				this.shippingMenuActive = true;
				if (Game1.activeClickableMenu is ShippingMenu)
				{
					base.Helper.Reflection.GetMethod(Game1.activeClickableMenu, "okClicked").Invoke();
				}
			}
		}

		private void OnUnvalidatedUpdateTick(object sender, UnvalidatedUpdateTickedEventArgs e)
		{
			if (Game1.timeOfDay >= this.Config.timeOfDayToSleep || (Game1.timeOfDay == 600 && this.currentDateForReset != this.danceOfJelliesForReset && this.currentDateForReset != this.spiritsEveForReset && this.Config.endofdayTimeOut != 0))
			{
				this.timeOutTicksForReset++;
				double countdowntoreset = (double)(2600 - this.Config.timeOfDayToSleep) * 0.01 * 6.0 * 7.0 * 60.0;
				if ((double)this.timeOutTicksForReset >= countdowntoreset + (double)(this.Config.endofdayTimeOut * 60))
				{
					Game1.options.setServerMode("offline");
				}
			}
			if ((this.currentDateForReset == this.danceOfJelliesForReset || (this.currentDateForReset == this.spiritsEveForReset && this.Config.endofdayTimeOut != 0)) && (Game1.timeOfDay >= 2400 || Game1.timeOfDay == 600))
			{
				this.timeOutTicksForReset++;
				if (this.timeOutTicksForReset >= 5040 + this.Config.endofdayTimeOut * 60)
				{
					Game1.options.setServerMode("offline");
				}
			}
			if (this.shippingMenuActive && this.Config.endofdayTimeOut != 0)
			{
				this.shippingMenuTimeoutTicks++;
				if (this.shippingMenuTimeoutTicks >= this.Config.endofdayTimeOut * 60)
				{
					Game1.options.setServerMode("offline");
				}
			}
			if (Game1.timeOfDay == 610)
			{
				this.shippingMenuActive = false;
				Game1.player.difficultyModifier = (float)this.Config.profitmargin * 0.01f;
				Game1.options.setServerMode("online");
				this.timeOutTicksForReset = 0;
				this.shippingMenuTimeoutTicks = 0;
			}
			if (Game1.timeOfDay == 2600)
			{
				Game1.paused = false;
			}
		}

		private void SendChatMessage(string message)
		{
			Game1.chatBox.activate();
			Game1.chatBox.setText(message);
			Game1.chatBox.chatBox.RecieveCommandInput('\r');
		}

		private void LeaveFestival()
		{
			Game1.netReady.SetLocalReady("festivalEnd", true);
			Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, delegate
			{
				this.GetBedCoordinates();
				Game1.exitActiveMenu();
				Game1.warpFarmer("Farmhouse", this.bedX, this.bedY, false);
				Game1.timeOfDay = ((this.currentDate == this.spiritsEve) ? 2400 : 2200);
				Game1.shouldTimePass();
			});
		}
	}
}
