using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTimeChat.Domain.Entities;
using RealTimeChat.Domain.Enums;

namespace RealTimeChat.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("Messages");

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.FileUrl)
            .HasMaxLength(500);

        builder.Property(x => x.SentAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsEdited)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(MessageType.Text);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(MessageStatus.Sent);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Chat)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sender)
            .WithMany(x => x.SentMessages)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Receipts)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SenderId);
        builder.HasIndex(x => x.ChatId);
        builder.HasIndex(x => x.SentAt);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new { x.ChatId, x.SentAt });
    }
}