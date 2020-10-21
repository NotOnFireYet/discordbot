using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace discordbot {

	public class CommandHandler {

		private DiscordSocketClient _client;

		private CommandService _service;

		public CommandHandler(DiscordSocketClient client) {
			_client = client;
			_service = new CommandService();

			_service.AddModulesAsync(Assembly.GetEntryAssembly());
			_client.MessageReceived += HandleCommandAsync;
		}

		char prefix = '!';

		private async Task HandleCommandAsync(SocketMessage s) {
			var msg = s as SocketUserMessage;

			var context = new SocketCommandContext(_client, msg);

			int argPos = 0;

			if (msg.HasCharPrefix(prefix, ref argPos)) {
				var result = await _service.ExecuteAsync(context, argPos);

				if (!result.IsSuccess && result.Error != CommandError.UnknownCommand) {
					await context.Channel.SendMessageAsync(result.ErrorReason);
				}
			}
		}

	}
}
