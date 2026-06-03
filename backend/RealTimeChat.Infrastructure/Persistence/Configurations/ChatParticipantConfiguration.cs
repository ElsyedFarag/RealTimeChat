using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTimeChat.Domain.Entities;

namespace RealTimeChat.Infrastructure.Persistence.Configurations;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("ChatParticipants");

        builder.HasKey(x => new { x.ChatId, x.UserId });

        builder.Property(x => x.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.IsAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Chat)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ChatParticipants)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ChatId);
        builder.HasIndex(x => x.JoinedAt);
    }
}