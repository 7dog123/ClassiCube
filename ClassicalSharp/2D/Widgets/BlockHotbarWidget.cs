﻿using System;
using System.Drawing;
using ClassicalSharp.Renderers;
using OpenTK.Input;

namespace ClassicalSharp {
	
	public sealed class BlockHotbarWidget : Widget {
		
		public BlockHotbarWidget( Game game ) : base( game ) {
			HorizontalDocking = Docking.Centre;
			VerticalDocking = Docking.BottomOrRight;
			hotbarCount = game.Inventory.Hotbar.Length;
		}
		
		int hotbarCount;
		Texture selectedBlock, background;
		const int blockSize = 40;
		
		public override bool HandlesKeyDown( Key key ) {
			if( key >= Key.Number1 && key <= Key.Number9 ) {
				game.Inventory.HeldBlockIndex = (int)key - (int)Key.Number1;
				return true;
			}
			return false;
		}
		
		static FastColour backCol = new FastColour( 60, 60, 60, 160 );
		static FastColour outlineCol = new FastColour( 169, 143, 192 );
		static FastColour selCol = new FastColour( 213, 200, 223 );
		public override void Init() {
			int width = blockSize * hotbarCount;
			X = game.Width / 2 - width / 2;
			Y = game.Height - blockSize;
			Width = width;
			Height = blockSize;
			MakeBackgroundTexture( width );
			MakeSelectionTexture();
		}
				
		public override void Render( double delta ) {
			graphicsApi.Texturing = true;
			background.Render( graphicsApi );
			// TODO: Maybe redesign this so we don't have to bind the whole atlas. Not cheap.
			graphicsApi.BindTexture( game.TerrainAtlas.TexId );
			
			bool setFog = game.EnvRenderer is StandardEnvRenderer;
			IsometricBlockDrawer.SetupState( graphicsApi, setFog );
			for( int i = 0; i < hotbarCount; i++ ) {
				int x = X + i * blockSize;
				IsometricBlockDrawer.Draw( game, (byte)game.Inventory.Hotbar[i], 10, 
				                          x + blockSize / 2, game.Height - 12 );
				if( i == game.Inventory.HeldBlockIndex )
					selectedBlock.X1 = x;
			}		
		
			IsometricBlockDrawer.RestoreState( graphicsApi, setFog );
			selectedBlock.Render( graphicsApi );
			graphicsApi.Texturing = false;
		}
		
		public override void Dispose() {
			graphicsApi.DeleteTexture( ref selectedBlock );
			graphicsApi.DeleteTexture( ref background );
		}
		
		public override void MoveTo( int newX, int newY ) {
			int deltaX = newX - X;
			int deltaY = newY - Y;
			X = newX;
			Y = newY;
			selectedBlock.X1 += deltaX;
			selectedBlock.Y1 += deltaY;
			background.X1 += deltaX;
			background.Y1 += deltaY;
		}
		
		void MakeBackgroundTexture( int width ) {
			Size size = new Size( width, blockSize );
			using( Bitmap bmp = IDrawer2D.CreatePow2Bitmap( size ) ) {
				using( IDrawer2D drawer = game.Drawer2D ) {
					drawer.SetBitmap( bmp );
					drawer.Clear( backCol );
					for( int xx = 0; xx < hotbarCount; xx++ ) {
						drawer.DrawRectBounds( outlineCol, 3, xx * blockSize, 0, blockSize, blockSize );
					}
					background = drawer.Make2DTexture( bmp, size, X, Y );
				}
			}
		}
		
		void MakeSelectionTexture() {
			Size size = new Size( blockSize, blockSize );
			using( Bitmap bmp = IDrawer2D.CreatePow2Bitmap( size ) ) {
				using( IDrawer2D drawer = game.Drawer2D ) {
					drawer.SetBitmap( bmp );
					drawer.DrawRectBounds( selCol, 3, 0, 0, blockSize, blockSize );
					selectedBlock = drawer.Make2DTexture( bmp, size, 0, Y );
				}
			}
		}
	}
}