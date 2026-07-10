# 3D Mesh Viewer — Project Plan (Project Phụ)
**WPF 3D / `Viewport3D` — Standalone project, KHÔNG chạy song song deadline 30/07 của Mini CAD Editor**

---

## Giả định & Phạm vi (Scope Assumptions)

- **Timeline độc lập:** Làm sau khi nộp JD TGL Solutions, hoặc dùng cho application khác nhấn mạnh 3D/graphics. Không tính vào 20 ngày của project chính.
- **OBJ Parser tự viết tay:** Không dùng thư viện có sẵn (HelixToolkit, AssimpNet...) — đúng tinh thần "tự hiện thực thuật toán xử lý dữ liệu" của JD.
- **Ray-picking tự viết:** Dùng thuật toán Möller–Trumbore (ray-triangle intersection) thay vì `VisualTreeHelper.HitTest` built-in của WPF, để thực sự show được thuật toán hình học 3D chứ không phải chỉ gọi API có sẵn.
- **Format hỗ trợ:** Chỉ OBJ cơ bản (`v`, `vt`, `vn`, `f` — tam giác hoặc quad, tự triangulate quad). Không cần support `.mtl` material/texture đầy đủ (để Post-MVP).
- **Scene:** Giả định 1 mesh object tại 1 thời điểm (không cần multi-object scene graph phức tạp cho MVP).

---

## 1. Tech Stack

| Layer | Công nghệ |
|---|---|
| UI Framework | WPF (.NET 8), `Viewport3D` / `ModelVisual3D` / `GeometryModel3D` |
| 3D Math | `Matrix3D`, `Transform3DGroup`, `Point3D`, `Vector3D` (built-in System.Windows.Media.Media3D) |
| Camera | `PerspectiveCamera` (tự viết orbit control logic) |
| Parsing | Tự viết `ObjParser` bằng `StreamReader` |
| Ray-picking | Tự viết Möller–Trumbore, không dùng HitTest built-in |
| Testing | xUnit |

---

## 2. Kiến trúc tổng thể

```
OBJ File (.obj)
      │
      ▼
ObjParser (tự viết)  ──►  Mesh { Vertices, Normals, Faces, AABB }
      │                         (AABB tính luôn lúc parse — dùng cho fit-to-view & ray broad-phase)
      ▼
MeshGeometry3D (convert để WPF render)
      │
      ▼
Viewport3D + PerspectiveCamera ──► OrbitCameraController (rotate/zoom/pan/fit-to-view)
      │
      ▼
Transform3DGroup (rotate/translate/scale object)
      │
Mouse Click ──► Screen-to-World Ray ──► AABB broad-phase check ──► Möller–Trumbore fine-phase (per triangle) ──► Highlight hit face
```

---

## 3. Cấu trúc thư mục

```
MeshViewer/
├─ Models/         → Mesh, Vertex, Face, BoundingBox
├─ Parsing/        → ObjParser
├─ Geometry3D/      → RayPicker, AABBUtils, RayTriangleIntersection
├─ Camera/          → OrbitCameraController
├─ ViewModels/      → MainViewModel
├─ Views/           → MainWindow.xaml
└─ Tests/           → MeshViewer.Tests (xUnit)
```

---

## 4. Git Strategy

Git Flow, branch từ `develop`:

- `feature/project-setup-3d`
- `feature/obj-parser`
- `feature/mesh-rendering`
- `feature/camera-orbit`
- `feature/transform-3d`
- `feature/ray-picking`
- `feature/ui-polish-3d`

---

## 5. Sprint Plan

### Sprint 1 — OBJ Parser & Static Rendering (Ngày 1–4)
**Goal:** Load 1 file OBJ, parse tay, render mesh tĩnh lên `Viewport3D` (chưa camera control, chưa transform — chỉ chứng minh pipeline chạy đúng).

**US-1.1: Project Setup**
- *Story:* Là developer, t cần solution WPF 3D setup sẵn, để build feature trên nền sạch.
- **Branch:** `feature/project-setup-3d`
- **AC:** Solution chạy được cửa sổ có `Viewport3D` trống; folder structure như mục 3; Git repo + push GitHub
- **Estimate:** 0.5 ngày

**US-1.2: OBJ Parser + AABB**
- *Story:* Là developer, t cần parser tự viết đọc được file `.obj` cơ bản, và tính sẵn bounding box trong lúc parse để dùng cho camera framing sau này.
- **Branch:** `feature/obj-parser`
- **AC:**
  - Đọc đúng dòng `v` (vertex), `vt` (texture coord — lưu nhưng chưa cần dùng), `vn` (normal), `f` (face)
  - Face có 4 đỉnh (quad) tự động triangulate thành 2 tam giác
  - Bỏ qua dòng comment (`#`) và dòng không hỗ trợ mà không crash
  - Trong lúc parse, track min/max X/Y/Z → tạo `BoundingBox` (AABB) của toàn mesh
  - Unit test: parse 1 OBJ string mẫu nhỏ (1 tam giác, 1 quad) → verify đúng số vertex/face và AABB đúng
- **Estimate:** 1.5 ngày

**US-1.3: Mesh → MeshGeometry3D Rendering**
- *Story:* Là user, t muốn thấy mesh hiện lên trong Viewport3D sau khi load file.
- **Branch:** `feature/mesh-rendering`
- **AC:**
  - Convert `Mesh` model sang `MeshGeometry3D` (Positions, TriangleIndices, Normals)
  - Gán `DiffuseMaterial` + ánh sáng cơ bản (`DirectionalLight` hoặc `AmbientLight`) để nhìn thấy khối 3D rõ ràng
  - Camera cố định tạm thời (chưa orbit) chỉ để verify mesh hiện đúng hình dạng
- **Estimate:** 1 ngày

**US-1.4: Sample Test Files**
- *Story:* Là developer, t cần vài file OBJ mẫu (cube, đơn giản) để test parser và rendering.
- **Branch:** `feature/obj-parser`
- **AC:** Có ít nhất 2-3 file `.obj` mẫu (cube tự viết tay, 1 model đơn giản tải từ nguồn public-domain) đặt trong `SampleFiles/`, load thử thành công cả 3
- **Estimate:** 0.5 ngày (bao gồm tìm/verify nguồn file public-domain hợp lệ)

---

### Sprint 2 — Camera Orbit & Transform (Ngày 5–7)
**Goal:** Camera điều khiển được bằng chuột; object xoay/di chuyển/scale được qua matrix transform.

**US-2.1: Orbit Camera**
- *Story:* Là user, t muốn giữ chuột trái kéo để xoay camera quanh object, để quan sát mesh từ mọi góc.
- **Branch:** `feature/camera-orbit`
- **AC:** Mouse drag (trái) → xoay camera quanh điểm target (tâm AABB từ US-1.2) theo yaw/pitch; không bị gimbal-lock ở góc gần 90°
- **Estimate:** 1.5 ngày

**US-2.2: Zoom & Pan**
- **Branch:** `feature/camera-orbit`
- **AC:** Scroll wheel → zoom in/out (giới hạn min/max distance); middle-click hoặc Shift+drag → pan camera
- **Estimate:** 0.5 ngày

**US-2.3: Fit-to-View / Reset**
- *Story:* Là user, t muốn 1 nút "Reset View" để camera tự canh giữa và vừa khung nhìn với mesh đang load, đặc biệt hữu ích khi load model mới có kích thước khác hẳn model cũ.
- **Branch:** `feature/camera-orbit`
- **AC:** Dùng `AABB` từ US-1.2 (tâm + kích thước) để tính vị trí/khoảng cách camera phù hợp; nút Reset gọi lại logic này bất kỳ lúc nào
- **Estimate:** 0.5 ngày

**US-2.4: Transform Object (Rotate/Translate/Scale)**
- *Story:* Là user, t muốn xoay/di chuyển/scale mesh bằng slider hoặc phím tắt, để chỉnh vị trí/kích thước object trong scene.
- **Branch:** `feature/transform-3d`
- **AC:**
  - `Transform3DGroup` áp lên `ModelVisual3D` của mesh, gồm `RotateTransform3D`, `TranslateTransform3D`, `ScaleTransform3D`
  - UI: sliders hoặc numeric input cho từng trục (X/Y/Z) của mỗi loại transform
  - Transform áp dụng real-time, không cần reload mesh
- **Estimate:** 1 ngày

---

### Sprint 3 — Ray-Picking (Ngày 8–11)
**Goal:** Click chuột lên mesh trong Viewport3D chọn đúng tam giác bị click, dùng thuật toán tự viết (không dùng HitTest built-in).

**US-3.1: Screen-to-World Ray**
- *Story:* Là developer, t cần convert tọa độ 2D của click chuột trên màn hình thành 1 tia (ray) 3D trong world space, dựa theo `PerspectiveCamera` hiện tại.
- **Branch:** `feature/ray-picking`
- **AC:** Hàm nhận `Point` (screen coords) + `Viewport3D` info (camera position, look direction, up direction, field of view, viewport size) → trả về `Ray3D { Origin, Direction }`; unit test với setup camera cố định, verify ray đi đúng hướng cho vài điểm mẫu (giữa màn hình, góc màn hình)
- **Estimate:** 1.5 ngày (phần dễ sai nhất — cần hiểu đúng pipeline camera transform của WPF)

**US-3.2: AABB Broad-Phase Check**
- *Story:* Là developer, t muốn loại nhanh trường hợp ray không chạm mesh, trước khi test từng tam giác — tránh lãng phí tính toán trên mesh lớn.
- **Branch:** `feature/ray-picking`
- **AC:** Hàm ray-AABB intersection (slab method), trả về `bool` sớm nếu ray không giao AABB tổng của mesh; unit test hit/miss case rõ ràng
- **Estimate:** 0.5 ngày

**US-3.3: Möller–Trumbore Ray-Triangle Intersection**
- *Story:* Là developer, t cần thuật toán chính xác tính điểm giao giữa ray và từng tam giác của mesh, để xác định đúng tam giác bị click.
- **Branch:** `feature/ray-picking`
- **AC:**
  - Implement đúng công thức Möller–Trumbore, trả về khoảng cách `t` nếu hit (và loại `t < 0`, tức giao phía sau camera)
  - Duyệt tất cả tam giác trong mesh (đã pass broad-phase ở US-3.2), lấy tam giác có `t` nhỏ nhất (gần camera nhất)
  - Unit test: tia bắn thẳng vào tâm tam giác (hit), tia song song mặt phẳng tam giác (miss, không chia cho 0), tia bắn trúng cạnh/đỉnh tam giác (edge case), tia bắn ra ngoài tam giác dù cùng mặt phẳng (miss)
- **Estimate:** 1.5 ngày

**US-3.4: Highlight Selected Face**
- *Story:* Là user, t muốn thấy tam giác vừa click được đổi màu để biết mình chọn đúng chỗ.
- **Branch:** `feature/ray-picking`
- **AC:** Tam giác trúng ray đổi `DiffuseMaterial` màu khác (vd: đỏ) tạm thời; click chỗ khác → bỏ highlight tam giác cũ, highlight tam giác mới
- **Estimate:** 0.5 ngày

---

### Sprint 4 — Polish & Submission (Ngày 12–13)
**Goal:** UI gọn gàng, README + demo sẵn sàng.

**US-4.1: UI Polish**
- **Branch:** `feature/ui-polish-3d`
- **AC:** Toolbar: nút "Load OBJ", "Reset View", toggle Wireframe/Solid render mode; status bar hiện số vertex/face của mesh đang load
- **Estimate:** 1 ngày

**US-4.2: README + Demo**
- **Branch:** `main`
- **AC:**
  - README: mô tả, tech stack, kiến trúc (copy sơ đồ mục 2), giải thích ngắn gọn thuật toán Möller–Trumbore đã dùng (thể hiện hiểu bản chất, không chỉ copy code)
  - Demo GIF/video: load file OBJ, orbit camera, transform object, click chọn 1 tam giác thấy highlight
  - Mục "Future Improvements": Octree cho mesh lớn (3D analog của QuadTree ở project 2D), `.mtl` material/texture support, multi-object scene
- **Estimate:** 0.5 ngày

**US-4.3: Final Push & Tag**
- **AC:** Merge `develop` → `main`, tag `v1.0`, clone sạch + build thử để chắc chắn chạy được trên máy khác
- **Estimate:** 0.5 ngày

---

## 6. Definition of Done

- [ ] Build không warning nghiêm trọng
- [ ] Unit test cho toàn bộ phần logic thuần (parser, AABB, ray-triangle intersection) — không cần test UI/rendering
- [ ] Test tay ít nhất 2-3 file OBJ khác kích thước
- [ ] Commit rõ ràng, merge qua PR vào `develop`

---

## 7. Testing Strategy

| Phần | Loại test | Ghi chú |
|---|---|---|
| ObjParser (v/vt/vn/f + triangulate quad) | Unit test | Test cả input hợp lệ và file có dòng comment/không hỗ trợ |
| AABB computation | Unit test | Verify min/max đúng với tập điểm biết trước |
| Ray-AABB broad-phase | Unit test | Hit/miss case rõ ràng |
| Möller–Trumbore | Unit test | Đây là phần quan trọng nhất — cần nhiều edge case (song song, cạnh, đỉnh, phía sau camera) |
| Screen-to-World Ray | Unit test | Setup camera cố định, verify hướng ray tại vài điểm mẫu |
| Camera orbit / rendering | Manual test | Không cần automated UI test |

---

## 8. Risk & Mitigation

| Risk | Khả năng | Mitigation |
|---|---|---|
| Screen-to-World ray tính sai do hiểu nhầm pipeline camera của WPF | Cao | Research kỹ `PerspectiveCamera` properties (Position, LookDirection, UpDirection, FieldOfView) trước khi code; viết unit test sớm để bắt lỗi ngay |
| Möller–Trumbore sai do floating-point precision | Trung bình | Dùng epsilon tolerance khi so sánh với 0; test kỹ edge case |
| WPF `Viewport3D` render chậm với mesh nhiều tam giác | Trung bình | Giới hạn file test mẫu (~vài chục nghìn triangle), ghi chú "future: LOD/mesh simplification" trong README thay vì cố tối ưu ngay |
| Orbit camera bị gimbal-lock ở góc pitch gần ±90° | Thấp | Clamp góc pitch trong khoảng an toàn (vd: -89° đến 89°) |

---

## 9. Post-MVP Backlog

- `.mtl` material/texture support (đọc màu/texture thật từ file)
- Octree cho spatial indexing khi mesh lớn — 3D analog trực tiếp của QuadTree đã làm ở Mini CAD Editor, dùng để so sánh trong interview: "t áp dụng lại tư duy spatial indexing từ project 2D sang 3D"
- Multi-object scene (nhiều mesh cùng lúc, mỗi object transform riêng)
- Export mesh đã transform ngược lại ra file `.obj`

---

## 10. Timeline Tổng Thể

| Ngày | Sprint | Deliverable chính |
|---|---|---|
| 1–4 | Sprint 1 | Parser + AABB + render mesh tĩnh |
| 5–7 | Sprint 2 | Camera orbit/zoom/pan/fit-to-view + transform object |
| 8–11 | Sprint 3 | Ray-picking hoàn chỉnh (Möller–Trumbore) |
| 12–13 | Sprint 4 | Polish, README, demo, push |

**Tổng ~13 ngày làm việc**, không tính buffer — nếu làm sau khi đã nộp Mini CAD Editor, đây coi như 1 project bồi thêm cho portfolio, không bị áp lực deadline 30/07.
