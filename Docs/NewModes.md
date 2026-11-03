# Các mode tích hợp từ match3_tool

Chỉ game chính được sửa. Project `match3_tool` là nguồn tham khảo, không phải dependency runtime.
Gameplay vẫn dùng `GameController`, `ItemTile`, pool, khay, ghép ba, animation và popup thắng/thua hiện tại.
Không cần gắn script hoặc prefab mới bằng Inspector: badge tile, tray mục tiêu và nhãn HUD được tạo runtime.

## Đối chiếu hai project

| ID | Mechanic | Trạng thái trong game chính |
| --- | --- | --- |
| 0 | Normal | Giữ nguyên |
| 1 | Random | Giữ nguyên |
| 2 | Double/link | Giữ nguyên |
| 3 | Freeze | Giữ nguyên, bao gồm mở băng bằng một tile kề |
| 4 | Gold | Giữ nguyên |
| 5 | 2-up | Giữ nguyên |
| 6 | Rainbow | Giữ nguyên |
| 7 | Fog | Giữ nguyên |
| 8 | Color | Chưa có mechanic hoàn chỉnh ở reference; CSV cũ có ID này và vẫn fallback về Normal như trước |
| 9 | Missing slot | Mới: giảm số ô khay khả dụng |
| 10 | Poison | Mới: ghép ba có poison sẽ ép thêm một tile thường vào khay rồi shuffle |
| 11 | Bomb | Mới: bomb đã lộ giảm lượt; hết lượt ép một tile lên khay cho mỗi bomb |
| 12 | Tray | Mới: mở hết các khay mục tiêu nằm dưới tile để thắng sớm |
| 13 | Winning tile | Mới: ghép bộ ba TARGET qua các đợt để thắng sớm |
| 14 | Gold coin | Mới: ghép hết các bộ ba COIN để thắng sớm |
| 15 | Conveyor | Chưa tích hợp: reference có 36 tile bổ sung, hai làn chuyển động và bố cục camera riêng |
| 100 | 3D | Chỉ thấy khai báo/reserved trong reference, chưa có mechanic đủ để port |

Reference còn hỗ trợ kết hợp nhiều mode trong một level. Game chính vẫn chạy một mode cho mỗi level;
không ngầm bỏ bớt mode khi nhập một tổ hợp.

## Cấu hình level

Giữ `data`, `tileCount`, `itemTypeCount`, `mode`, `param` đang dùng. `bombMoves` là trường tùy chọn mới.

| mode | param | Luật và điều kiện kết thúc |
| --- | --- | --- |
| 9 | Số ô bị khóa; mặc định 1, giới hạn 1–4 | Còn 6–3 ô trong khay. Ghép ba/dọn hết board để thắng; đầy khay không có match thì thua như trước |
| 10 | Tổng tile poison; mặc định 3, không vượt số tile thường | Bất kỳ triple chứa poison đều ép một tile thường ngẫu nhiên lên khay rồi shuffle phần còn trên board. Thắng/thua theo luật board/khay cũ |
| 11 | Số bomb mỗi tầng; mặc định 1 | `bombMoves` mặc định 5. Chỉ bomb lộ trên board giảm lượt; tile vừa được chọn không giảm. Bomb về 0 kích hoạt một lần, ép thêm một tile thường nếu còn chỗ. Thu thập/ghép bomb trước khi về 0 tránh kích hoạt. Thắng/thua theo luật cũ |
| 12 | Số khay mục tiêu; mặc định 3, giới hạn số tầng không rỗng | Mỗi tầng được chọn có một khay 2×2 dưới tile. Mở tất cả khay để thắng; phần tile còn lại tự thu dọn. Khay đã mở giữ trạng thái khi Undo |
| 13 | Số đợt TARGET; mặc định 3 | Mỗi đợt là 3 tile chung một loại được chuyển thành TARGET. Ghép đủ đợt để thắng. Nếu không còn một bộ ba đầy đủ trên board để tạo đợt tiếp theo, tiếp tục dọn board theo luật thường |
| 14 | Số tile COIN; mặc định 9, làm tròn xuống bộ ba, tối thiểu một bộ, giới hạn số triple có sẵn | Ghép hết COIN để thắng, rồi tự thu dọn tile còn lại. COIN là mục tiêu màn chơi, không tự cộng thêm tiền ngoài phần thưởng thắng hiện tại |

Bomb đếm một lượt mỗi lần chọn tay và một lượt mỗi lần dùng Hint; các tile bị ép lên khay không tự giảm thêm lượt.
Undo không hoàn lại lượt bomb đã tiêu. Countdown, trạng thái bomb đã kích hoạt, số đợt TARGET và khay đã mở được lưu.

Ví dụ một level bomb, dữ liệu board theo định dạng game chính:

```json
{"data":["111111111111111111000000"],"tileCount":18,"itemTypeCount":3,"mode":11,"param":2,"bombMoves":5}
```

Đổi `mode`/`param` theo bảng để tạo các màn thử nghiệm khác. Reader cũng chấp nhận `"mode":[11],"param":[2]`.
Array có nhiều mode bị từ chối bằng lỗi rõ ràng. ID 15 và 100 bị từ chối khi import JSON.

Đây chỉ là tương thích header mode/param. Board editor của reference có quy tắc kích thước/tầng khác;
không đưa trực tiếp dữ liệu bố cục từ tool sang rồi mặc định vị trí sẽ giống nhau.

Không thay đổi CSV hoặc `Levels.asset` hiện có trong lần tích hợp này. Để thử, dùng bản sao `LevelsSO`
hoặc thêm level mới ở cuối danh sách. Có thể dùng ô Enter level tại Home để vào level vừa tạo.
`LevelsSO.LevelSetup` đọc lại toàn bộ danh sách từ CSV; không gọi trên asset đang dùng nếu chưa chuẩn bị dữ liệu đầy đủ.

## Tương thích và tài nguyên

- Enum 0–8, mode index cũ, save key và save version 1 được giữ nguyên.
- Save mới chỉ thêm payload tùy chọn. Save của mode cũ không cần có payload này.
- Type -1/-2 cũ giữ nguyên; TARGET dùng -3, COIN dùng -4.
- TARGET/COIN lấy theo triple hoàn chỉnh để không để sót tile thường không thể ghép.
- Hiệu ứng poison/bomb được xử lý khi tile đã hạ khay và match đang chờ đã xong; input bị khóa trong hiệu ứng.
- Trong hiệu ứng, tiếp tục giữ snapshot save ổn định trước đó thay vì ghi trạng thái dở dang.
- Hint không được đưa game từ WIN/LOSE trở lại PLAYING ở mode mới.
- Badge `POISON`, `BOMB n`, `TARGET`, `COIN`, dấu X ô khóa và khay màu cam tái sử dụng TMP/sprite hiện tại.
  Đây là visual chức năng, chưa có bộ art chuyên dụng như project tham khảo.

## Kiểm tra tự động

Chạy từ PowerShell tại project chính:

```powershell
./Tests/Run-NewModeChecks.ps1 -UnityInstall E:/unity/6000.6.4f1
```

Nếu PowerShell chặn chạy file script, có thể chạy riêng tiến trình kiểm tra mà không đổi policy hệ thống:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tests/Run-NewModeChecks.ps1 -UnityInstall E:/unity/6000.6.4f1
```

Script dùng Roslyn và response file của Unity trong Library để kiểm tra C# của game, sau đó chạy
`Tests/NewModeChecks.cs` với assembly thực. Output chỉ ghi vào `Temp/mode-integration`.
Không khởi động Unity Editor, không tạo APK/AAB, không commit/push và không sửa project tham khảo.

Các kiểm tra bao gồm mapping enum cũ/mới, đọc toàn bộ CSV cũ, scalar/array đơn, từ chối tổ hợp chưa hỗ trợ,
giá trị mặc định, roundtrip DTO JSON, countdown bomb và bảo toàn số lượng triple khi tạo mục tiêu.
Kiểm tra DTO JSON độc lập không thay thế kiểm thử save bằng JsonUtility trong Unity.

## Checklist kiểm thử thủ công trong Unity

1. Đợi Unity import script mới, kiểm tra Console và mở GameScene; không cần thêm prefab reference.
2. Chạy lại mode 0–7 và level ID 8 cũ. Kiểm tra freeze, Hint, Undo, Shuffle, revive, event và save cũ.
3. Mode 9: thử khóa 1 và 4 ô; kiểm tra full tray, match đang bay và vị trí dấu X.
4. Mode 10: ghép poison, kiểm tra ép đúng một tile, shuffle không đổi tile đã ở khay; thử ép tile gây match hoặc thua.
5. Mode 11: thử bomb lộ/ẩn, chọn chính bomb, bomb hết lượt đồng thời, Hint, Undo và save/resume.
6. Mode 12: thử nhiều tầng, tile lệch nửa ô, mở một phần rồi Undo; khôi phục save còn khay chưa mở.
7. Mode 13: hoàn thành 3 đợt, dùng Hint ở đợt cuối, thử board nhỏ thiếu triple cho đợt tiếp theo.
8. Mode 14: ghép hết COIN khi board còn tile thường và khi khay gần đầy; xác nhận chỉ hiện một popup thắng.
9. Mỗi mode mới: thoát ứng dụng rồi resume, pause giữa animation, về Home/replay và kiểm tra badge không sót trên tile pool.
10. Kiểm tra vị trí HUD, tutorial, badge và khay mục tiêu trên màn hình hẹp/rộng và thiết bị mobile.

Chỉ sau Play Mode/device test mới có thể xác nhận collider, animation, native JsonUtility và UI chạy đúng trong Unity.
