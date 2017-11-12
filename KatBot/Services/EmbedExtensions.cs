using System.Threading.Tasks;
using Discord;

namespace KatBot.Services.EmbedExtensions
{
    public static class EmbedExtensions
    {
        public static async Task SendConfirmAsync(this IMessageChannel Channel, string message)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(111, 237, 69);
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendConfirmAsync(this IMessageChannel Channel, string title, string message)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(111, 237, 69);
            builder.Title = title;
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendErrorAsync(this IMessageChannel Channel, string message)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(222, 90, 47);
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendErrorAsync(this IMessageChannel Channel, string title, string message)
        {
            var builder = new EmbedBuilder();

            builder.Color = new Color(222, 90, 47);
            builder.Title = title;
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendColouredEmbedAsync(this IMessageChannel Channel, string message, Color color)
        {
            var builder = new EmbedBuilder();

            builder.Color = color;
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendColouredEmbedAsync(this IMessageChannel Channel, string title, string message,
            Color color)
        {
            var builder = new EmbedBuilder();

            builder.Color = color;
            builder.Title = title;
            builder.Description = message;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendImageEmbedAsync(this IMessageChannel Channel, string URL)
        {
            var builder = new EmbedBuilder();

            builder.Color = Katarina.KatClr;
            builder.ImageUrl = URL;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendImageEmbedAsync(this IMessageChannel Channel, string URL, string Title)
        {
            var builder = new EmbedBuilder();

            builder.Color = Katarina.KatClr;
            builder.Title = Title;
            builder.ImageUrl = URL;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }

        public static async Task SendImageEmbedWithoutTitleAsync(this IMessageChannel Channel, string URL, string text)
        {
            var builder = new EmbedBuilder();

            builder.Color = Katarina.KatClr;
            builder.Description = text;
            builder.ImageUrl = URL;

            await Channel.SendMessageAsync("", embed: builder.Build());
        }
    }
}