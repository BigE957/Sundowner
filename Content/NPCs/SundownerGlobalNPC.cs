using CalamityMod;
using CalamityMod.NPCs.CeaselessVoid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sundowner.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Sundowner.Content.NPCs
{
    public class SundownerGlobalNPC : GlobalNPC
    {
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (npc.type == ModContent.NPCType<CeaselessVoid>() && npc.life <= 0)
                CeaselessVoidDeathSystem.OnCeaselessDeath();
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type != ModContent.NPCType<CeaselessVoid>() || ModCompat.InfernumMode != null)
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

            // Draw Code for Ceaseless Void gotten directly from Calamity, modified for the death animation

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture2D15 = TextureAssets.Npc[npc.type].Value;
            Vector2 halfSizeTexture = new((TextureAssets.Npc[npc.type].Value.Width / 2), (TextureAssets.Npc[npc.type].Value.Height / Main.npcFrameCount[npc.type] / 2));
            
            float rotation = npc.rotation;
            Vector2 deathOffset = Vector2.Zero;

            if (((CeaselessVoid)npc.ModNPC).playedbuildsound)
            {
                float currentSpeed = CeaselessVoidDeathSystem.speedUp.Speed - 1;
                rotation += Main.rand.NextFloat(-currentSpeed, currentSpeed) / 4f;
                deathOffset = Main.rand.NextVector2CircularEdge(currentSpeed, currentSpeed) * 4f;
            }

            int afterimageAmt = 7;

            if (CalamityConfig.Instance.Afterimages)
            {
                for (int i = 1; i < afterimageAmt; i += 2)
                {
                    Color afterimageColor = drawColor;
                    afterimageColor = Color.Lerp(afterimageColor, Color.White, 0.5f);
                    afterimageColor = npc.GetAlpha(afterimageColor);
                    afterimageColor *= (float)(afterimageAmt - i) / 15f;
                    
                    Vector2 afterimageDrawPos = npc.oldPos[i] + new Vector2((float)npc.width, (float)npc.height) / 2f - screenPos;
                    afterimageDrawPos -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
                    afterimageDrawPos += halfSizeTexture * npc.scale + new Vector2(0f, npc.gfxOffY);
                    afterimageDrawPos += deathOffset;

                    spriteBatch.Draw(texture2D15, afterimageDrawPos, npc.frame, afterimageColor, rotation, halfSizeTexture, npc.scale, spriteEffects, 0f);
                }
            }

            Vector2 drawLocation = npc.Center - screenPos;
            drawLocation -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
            drawLocation += halfSizeTexture * npc.scale + new Vector2(0f, npc.gfxOffY);
            drawLocation += deathOffset;

            spriteBatch.Draw(texture2D15, drawLocation, npc.frame, npc.GetAlpha(drawColor), rotation, halfSizeTexture, npc.scale, spriteEffects, 0f);

            texture2D15 = CeaselessVoid.GlowTexture.Value;
            Color cyanLerp = Color.Lerp(Color.White, Color.Cyan, 0.5f);

            if (CalamityConfig.Instance.Afterimages)
            {
                for (int j = 1; j < afterimageAmt; j++)
                {
                    Color extraAfterimageColor = cyanLerp;
                    extraAfterimageColor = Color.Lerp(extraAfterimageColor, Color.White, 0.5f);
                    extraAfterimageColor *= (float)(afterimageAmt - j) / 15f;
                    
                    Vector2 extraAfterimageDrawPos = npc.oldPos[j] + new Vector2((float)npc.width, (float)npc.height) / 2f - screenPos;
                    extraAfterimageDrawPos -= new Vector2((float)texture2D15.Width, (float)(texture2D15.Height / Main.npcFrameCount[npc.type])) * npc.scale / 2f;
                    extraAfterimageDrawPos += halfSizeTexture * npc.scale + new Vector2(0f, npc.gfxOffY);
                    extraAfterimageDrawPos += deathOffset;

                    spriteBatch.Draw(texture2D15, extraAfterimageDrawPos, npc.frame, extraAfterimageColor, rotation, halfSizeTexture, npc.scale, spriteEffects, 0f);
                }
            }

            spriteBatch.Draw(texture2D15, drawLocation, npc.frame, cyanLerp, rotation, halfSizeTexture, npc.scale, spriteEffects, 0f);

            return false;
        }
    }
}
