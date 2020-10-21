using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Diagnostics;
using System.IO;

namespace discordbot {
	class Program {
		static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		DiscordSocketClient client;

		public async Task MainAsync() {

			DiscordSocketConfig cfg = new DiscordSocketConfig();
			cfg.MessageCacheSize = 100;

			client = new DiscordSocketClient(cfg);

			new CommandHandler(client);

			client.Log += Log;

			string token = "NTEyMTg5NDY5MDgwOTQ0NjUx.Ds176Q.JAtI42ov1ObKgV0vhAl1fmgcmeI";
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			client.ReactionAdded += ReactionAdded;
			//client.MessageReceived += MessageReceived;

			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg) {
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		ulong start_ID;
		ulong end_ID;
		ulong target_server_id;

		string target_channel;
		ulong target_channel_id;

		Optional<IUser> user;
		string date;
		bool pin_started;


		string directory = @"C:\Users\User\Desktop\Everlasting Hug\C#\discordbot\discordbot\";
		string fileName = "archived.txt";

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction) {

			if (reaction.Emote.Name == "🇸") {
				string archived = File.ReadAllText(directory + fileName);

				if (!archived.Contains(reaction.MessageId.ToString())) {
					pin_started = true;
					start_ID = reaction.MessageId;

					target_channel = reaction.Channel.Name;
					user = reaction.User;

					archived += " " + reaction.MessageId.ToString();
					File.WriteAllText(directory + fileName, archived);

					Debug.WriteLine(start_ID);
				}
			}

			if (reaction.Emote.Name == "🇪") {

				if (pin_started) {
					end_ID = reaction.MessageId;
					Debug.WriteLine(end_ID);

					var messages = await reaction.Channel.GetMessagesAsync(end_ID, Direction.Before, 20).Flatten();

					List<IMessage> result = new List<IMessage>();
					IMessage last_mess = await reaction.Channel.GetMessageAsync(end_ID);
					result.Add(last_mess);

					int count = 0;

					foreach (var msg in messages) {
						result.Add(msg);
						count++;

						if (msg.Id == start_ID) {
							break;
						}
					}

					if (count >= 20) {
						await reaction.Channel.SendMessageAsync("Whoa, chill. I only archive 2 to 20 messages!");
						pin_started = false;
					}
					else {
						result.Reverse();
						List<Embed> res = make_it_pretty(result);

						target_server_id = 518022980270555138; //testing grounds 2.0
						//target_server_id = 313037935287074816; //archive

						IMessage mess = await reaction.Channel.GetMessageAsync(end_ID);

						int day = mess.Timestamp.Day;
						int month = mess.Timestamp.Month;
						int year = mess.Timestamp.Year;

						date = day + "/" + month + "/" + year;

						foreach (Embed msg in res) {
							await send_final_message(msg, target_server_id);
						}

						await client.GetGuild(target_server_id).GetTextChannel(target_channel_id).SendMessageAsync("Archived by *" + user + "*\n*" + date + "*");

						foreach (Embed msg in res) {
							await reaction.Channel.SendMessageAsync("", false, msg);
						}
						await reaction.Channel.SendMessageAsync("Archived by *" + user + "*\n*" + date + "*");

						pin_started = false;
					}
				}
			}
		}

		private async Task send_final_message(Embed final, ulong server_id) { //sends the message to the right server
			SocketGuildChannel[] chans = client.GetGuild(server_id).Channels.ToArray();
			
			ulong target_ID = 0;

			foreach (SocketGuildChannel chan in chans) {
				if (chan.Name == target_channel) {
					target_ID = chan.Id;
					target_channel_id = target_ID;
					break;
				}
			}
			await client.GetGuild(server_id).GetTextChannel(target_ID).SendMessageAsync("", false, final);
		}

		private List<Embed> make_it_pretty(List<IMessage> input) { //makes a list of embeds

			List<Embed> res = new List<Embed>();
			string usname;
			string prev_us = "";
			string text = "";

			for (int i = 0; i < input.Count; i++) {
				usname = input[i].Author.Username;
				List<IAttachment> attachments = new List<IAttachment>(); 

				if (usname != prev_us) { //if new user starts
					text = "";
					color_picked = false;

					if (i < input.Count - 1) { //if not last ever
						if (input[i + 1].Author.Username != usname) {//if last of user
							text += "\n" + input[i].Content;
							string icon = input[i].Author.GetAvatarUrl();

							var builder = new EmbedBuilder();
							builder = build_message(usname, text, icon);

							res.Add(builder);

							if (input[i].Attachments.FirstOrDefault() != null) {
								IAttachment att = input[i].Attachments.FirstOrDefault();
								res.Add(build_attachment(att));
							}
						}
						else {//if not last of user
							if (input[i].Attachments.FirstOrDefault() != null) {
								text += "\n" + input[i].Content;
								string icon = input[i].Author.GetAvatarUrl();

								var builder = new EmbedBuilder();
								builder = build_message(usname, text, icon);

								res.Add(builder);

								EmbedBuilder build_pic = build_attachment(input[i].Attachments.FirstOrDefault());
								res.Add(build_pic);

								text = "";
							} else
								text += "\n" + input[i].Content;
						}
					}
					else {//if last ever
						text += "\n" + input[i].Content;
						string icon = input[i].Author.GetAvatarUrl();

						var builder = new EmbedBuilder();
						builder = build_message(usname, text, icon);

						res.Add(builder);

						if (input[i].Attachments.FirstOrDefault() != null) {
							IAttachment att = input[i].Attachments.FirstOrDefault();
							res.Add(build_attachment(att));
						}
					}
				}
				else { //if old user continues
					if (i < input.Count - 1) { //if not last ever
						if (input[i + 1].Author.Username != usname) {//if last of user
							text += "\n" + input[i].Content;
							string icon = input[i].Author.GetAvatarUrl();

							var builder = new EmbedBuilder();
							builder = build_message(usname, text, icon);

							res.Add(builder);

							if (input[i].Attachments.FirstOrDefault() != null) {
								IAttachment att = input[i].Attachments.FirstOrDefault();
								res.Add(build_attachment(att));
							}
						}
						else {
							if (input[i].Attachments.FirstOrDefault() != null) {
								text += "\n" + input[i].Content;
								string icon = input[i].Author.GetAvatarUrl();

								var builder = new EmbedBuilder();
								builder = build_message(usname, text, icon);

								res.Add(builder);

								EmbedBuilder build_pic = build_attachment(input[i].Attachments.FirstOrDefault());
								res.Add(build_pic);

								text = "";
							}

							else
								text += "\n" + input[i].Content;
						}
					}
					else { //if it's the last one
						text += "\n" + input[i].Content;
						string icon = input[i].Author.GetAvatarUrl();

						var builder = new EmbedBuilder();
						builder = build_message(usname, text, icon);

						res.Add(builder);

						if (input[i].Attachments.FirstOrDefault() != null) {
							IAttachment att = input[i].Attachments.FirstOrDefault();
							res.Add(build_attachment(att));
						}
					}
				}

				prev_us = input[i].Author.Username;
			}

			return res;
		}

		Color rnd_color;
		bool color_picked;

		private EmbedBuilder build_attachment(IAttachment attachment) {
			var image_build = new EmbedBuilder();
			image_build.WithImageUrl(attachment.Url);

			if (color_picked) {
				image_build.WithColor(rnd_color);
			} else {
				Random rnd = new Random();
				rnd_color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
				image_build.WithColor(rnd_color);

				color_picked = true;
			}
			return image_build;
		}

		private EmbedBuilder build_message(string usname, string text, string icon) { //makes the embed for message
			var auth_build = new EmbedAuthorBuilder();
			auth_build.WithName(usname);
			auth_build.WithIconUrl(icon);

			var builder = new EmbedBuilder();
			builder.WithAuthor(auth_build);
			builder.WithDescription(text);

			if (color_picked) {
				builder.WithColor(rnd_color);
			}
			else {
				Random rnd = new Random();
				rnd_color = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256));
				builder.WithColor(rnd_color);

				color_picked = true;
			}

			return builder;
		}

		//private async Task MessageReceived(SocketMessage message) {
		//	if (!message.Author.IsBot) {
		//		await message.Channel.SendMessageAsync("yes");
		//	}

		//}

	}
}
