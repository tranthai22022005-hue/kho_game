Snake Classic Audio Pack - Unity

Files:
1. snake_eat_normal.wav
   - Gắn vào Eat Clip trong SnakeGameManager.
   - Dùng khi rắn ăn thức ăn.

2. snake_speed_boost.wav
   - Có thể dùng nếu bạn mở rộng âm riêng cho item SpeedBoost.

3. snake_slow_item.wav
   - Có thể dùng nếu bạn mở rộng âm riêng cho item Slow.

4. snake_game_over.wav
   - Gắn vào Game Over Clip trong SnakeGameManager.

5. ui_button_click.wav
   - Có thể dùng cho âm click button nếu mở rộng UI.

6. menu_music_loop.wav
   - Có thể dùng làm nhạc nền menu nếu tạo thêm AudioSource riêng.

Unity Import gợi ý:
- Hiệu ứng ngắn: Load Type = Decompress On Load, Preload Audio Data = On.
- Nhạc nền: Load Type = Streaming hoặc Compressed In Memory, Loop = On trong AudioSource.
- AudioSource trên GameManager: Play On Awake = Off, Loop = Off, Spatial Blend = 0.
