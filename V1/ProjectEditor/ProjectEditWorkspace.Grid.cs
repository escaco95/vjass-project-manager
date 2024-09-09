using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.Config;
using vJassMainJBlueprint.V1.ModelFacade;
using vJassMainJBlueprint.V1.ProjectEditor.Elements;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        private void OnFooterZoomGridDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }


        private static readonly string[] separator = ["\r\n", "\n"];

        private void OnFooterZoomGridDrop(object sender, DragEventArgs e)
        {
            // Text 형식으로 파일 경로가 전달될 수 있음
            // e.g) VSCode에서 파일을 드래그 앤 드롭할 경우
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                try
                {
                    string filePathString = (string)e.Data.GetData(DataFormats.Text);

                    if (string.IsNullOrWhiteSpace(filePathString)) return;

                    string[] filePaths = filePathString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    if (!filePaths.All(File.Exists)) return;

                    var dropWorldPosition = e.GetPosition(FooterZoomGrid);

                    var addedFileCount = ProjectSourceAdd([.. filePaths], (int)dropWorldPosition.X, (int)dropWorldPosition.Y);
                    MessageText.Info($"{addedFileCount}개의 파일을 추가했습니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"텍스트로 파일 경로를 처리하는 중 오류가 발생했습니다: {ex.Message}");
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files.Length == 0) return;

                    var dropWorldPosition = e.GetPosition(FooterZoomGrid);

                    var addedFileCount = ProjectSourceAdd([.. files], (int)dropWorldPosition.X, (int)dropWorldPosition.Y);
                    MessageText.Info($"{addedFileCount}개의 파일을 추가했습니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 추가하는 중 오류가 발생했습니다: {ex.Message}");
                }
            }
        }

        private readonly Dictionary<long, bool> selectedNodeIds = [];
        private double selectionRectPoint1X = 0;
        private double selectionRectPoint1Y = 0;
        private double selectionRectPoint2X = 0;
        private double selectionRectPoint2Y = 0;
        private bool isResizing = false;
        private bool isDragging = false;
        private bool isVieportDragging = false;
        private ElemNode? editingNode = null;
        private readonly Dictionary<ElemNode, Point> lastMouseOffsets = [];
        private Point lastMousePosition = new(0, 0);
        private ResizeGripDirection lastResizeDirection = ResizeGripDirection.None;

        private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    {
                        Point mousePosition = e.GetPosition(ZoomChild);
                        // 마우스 위치와 겹치는 TopMost 노드 선택
                        ElemNode? topMostNode = NodeContainer.Children.OfType<ElemNode>()
                            .Where(node => new Rect(Canvas.GetLeft(node), Canvas.GetTop(node), node.ActualWidth, node.ActualHeight).Contains(mousePosition))
                            .OrderBy(node => Panel.GetZIndex(node))
                            .FirstOrDefault();
                        if (projectEditFacade.SelectNodeSourceFilePath(topMostNode?.SourceNodeID) is string sourceFilePath)
                        {
                            // 상대 경로인 경우 문서 기준 절대 경로로 변환
                            string absoluteSourceFilePath = PathHelper.ConvertToAbsolutePath(projectEditFacade.OriginFilePath, sourceFilePath);
                            Debug.WriteLine($"Open: {absoluteSourceFilePath}");
                            ProcessHelper.Open(Window.GetWindow(this), absoluteSourceFilePath);
                        }
                    }
                    break;
                case MouseButton.Middle:
                    {
                        Point mousePosition = e.GetPosition(ZoomChild);
                        // 마우스 위치와 겹치는 TopMost 노드 선택
                        ElemNode? topMostNode = NodeContainer.Children.OfType<ElemNode>()
                            .Where(node => new Rect(Canvas.GetLeft(node), Canvas.GetTop(node), node.ActualWidth, node.ActualHeight).Contains(mousePosition))
                            .OrderBy(node => Panel.GetZIndex(node))
                            .FirstOrDefault();
                        if (topMostNode is null)
                        {
                            isVieportDragging = true;
                            lastMousePosition = e.GetPosition(ZoomParent);
                            WorkspaceGrid.CaptureMouse(); // 마우스 캡처

                            // 찍은 좌표 노출
                            MessageText.Info($"X:{(int)mousePosition.X} Y:{(int)mousePosition.Y}");
                            break;
                        }
                        // 선택되지 않은 노드를 클릭 시, 선택된 노드 초기화
                        if (!selectedNodeIds.ContainsKey(topMostNode.SourceNodeID))
                        {
                            SelectionClear();
                            SelectionAdd(topMostNode.SourceNodeID);
                        }
                        // 드래그 또는 크리 조정 시작
                        var resizeDirection = UIResizeHelper.GetResizeDirection(topMostNode, e.GetPosition(topMostNode));
                        if (resizeDirection == ResizeGripDirection.None)
                        {
                            isDragging = true;
                            editingNode = topMostNode;
                            lastMouseOffsets.Clear();
                            foreach (long selectedNodeHandleId in selectedNodeIds.Keys)
                            {
                                var selectedNode = nodeElements[selectedNodeHandleId];
                                lastMouseOffsets[selectedNode] = e.GetPosition(selectedNode);
                            }
                            topMostNode.CaptureMouse(); // 마우스 캡처
                            // 스냅 커서 표시
                            SnapCursor.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            isResizing = true;
                            editingNode = topMostNode;
                            lastMousePosition = mousePosition;
                            lastResizeDirection = resizeDirection;
                            lastMouseOffsets.Clear();
                            foreach (long selectedNodeHandleId in selectedNodeIds.Keys)
                            {
                                var selectedNode = nodeElements[selectedNodeHandleId];
                                lastMouseOffsets[selectedNode] = new Point(selectedNode.ActualWidth, selectedNode.ActualHeight);
                            }
                            topMostNode.CaptureMouse(); // 마우스 캡처
                        }
                    }
                    break;
                case MouseButton.Right:
                    {
                        // 마우스 좌클릭 시, 선택된 노드 초기화
                        SelectionClear();
                        WorkspaceGrid.CaptureMouse();
                        Point mousePosition = e.GetPosition(ZoomChild);
                        selectionRectPoint1X = mousePosition.X;
                        selectionRectPoint1Y = mousePosition.Y;
                        selectionRectPoint2X = mousePosition.X;
                        selectionRectPoint2Y = mousePosition.Y;
                        double zoomFactor = ((ScaleTransform)ZoomChild.LayoutTransform).ScaleX;
                        Canvas.SetLeft(SelectionGrid, Canvas.GetLeft(ZoomChild) + selectionRectPoint1X * zoomFactor);
                        Canvas.SetTop(SelectionGrid, Canvas.GetTop(ZoomChild) + selectionRectPoint1Y * zoomFactor);
                        SelectionGrid.Width = 0;
                        SelectionGrid.Height = 0;
                        SelectionGrid.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void OnGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    {
                        if (isDragging)
                        {
                            projectEditFacade.UpdateNodePosition(
                                selectedNodeIds.Keys
                                    .Select(nodeHandleId => nodeElements[nodeHandleId])
                                    .Select(node =>
                                    new ProjectEditFacade.NodePositionUpdateRequest
                                    {
                                        NodeHandleId = node!.SourceNodeID,
                                        X = (int)Canvas.GetLeft(node),
                                        Y = (int)Canvas.GetTop(node),
                                    }).ToList());
                            projectEditFacade.UpdateOriginSaveRequired(true);
                            editingNode?.ReleaseMouseCapture(); // 마우스 캡처 해제
                            // 스냅 커서 숨김
                            SnapCursor.Visibility = Visibility.Collapsed;
                            // 만약 하나의 객체만 선택되어 있었을 경우, 선택 해제
                            if (selectedNodeIds.Count == 1)
                            {
                                SelectionClear();
                            }
                        }
                        else if (isResizing)
                        {
                            projectEditFacade.UpdateNodeSize(
                                selectedNodeIds.Keys
                                    .Select(nodeHandleId => nodeElements[nodeHandleId])
                                    .Select(node =>
                                    new ProjectEditFacade.NodeSizeUpdateRequest
                                    {
                                        NodeHandleId = node!.SourceNodeID,
                                        Width = (int)node!.ActualWidth,
                                        Height = (int)node!.ActualHeight,
                                    }).ToList());
                            projectEditFacade.UpdateOriginSaveRequired(true);
                            editingNode?.ReleaseMouseCapture(); // 마우스 캡처 해제
                            // 만약 하나의 객체만 선택되어 있었을 경우, 선택 해제
                            if (selectedNodeIds.Count == 1)
                            {
                                SelectionClear();
                            }
                        }
                        else if (isVieportDragging)
                        {
                            WorkspaceGrid.ReleaseMouseCapture(); // 마우스 캡처 해제
                        }
                        isDragging = false;
                        isResizing = false;
                        isVieportDragging = false;
                    }
                    break;
                case MouseButton.Right:
                    {
                        // 마우스 선택 영역에 맞게 사각형 생성 (마우스 드래그 방향에 따라 Point1과 Point2를 조정)
                        double left = Math.Min(selectionRectPoint1X, selectionRectPoint2X);
                        double top = Math.Min(selectionRectPoint1Y, selectionRectPoint2Y);
                        double width = Math.Abs(selectionRectPoint1X - selectionRectPoint2X);
                        double height = Math.Abs(selectionRectPoint1Y - selectionRectPoint2Y);
                        Rect selectionRect = new(left, top, width, height);
                        // 선택 영역에 포함된 노드 선택
                        SelectionAdd(NodeContainer.Children.OfType<ElemNode>()
                            .Where(node => selectionRect.IntersectsWith(new Rect(Canvas.GetLeft(node), Canvas.GetTop(node), node.ActualWidth, node.ActualHeight)))
                            .Where(node => !selectedNodeIds.ContainsKey(node.SourceNodeID))
                            .Select(node => node.SourceNodeID)
                            .ToList());
                        // 마우스 좌클릭 해제 시, 마우스 캡쳐 해제
                        WorkspaceGrid.ReleaseMouseCapture();
                        SelectionGrid.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        private void OnGridMouseMove(object sender, MouseEventArgs e)
        {
            if (isVieportDragging)
            {
                Point currentPosition = e.GetPosition(ZoomParent);

                Mouse.OverrideCursor = Cursors.ScrollAll;

                Canvas.SetLeft(ZoomChild, Canvas.GetLeft(ZoomChild) - lastMousePosition.X + currentPosition.X);
                Canvas.SetTop(ZoomChild, Canvas.GetTop(ZoomChild) - lastMousePosition.Y + currentPosition.Y);

                lastMousePosition = currentPosition;
            }
            else if (isDragging)
            {
                const int gridSize = JassProjectSpecification.SnapThreshold;
                foreach (var item in lastMouseOffsets)
                {
                    Point currentPosition = e.GetPosition(ZoomChild);
                    // snap to grid (10x10 based on parent element offset)
                    currentPosition.X = currentPosition.X - item.Value.X;
                    currentPosition.Y = currentPosition.Y - item.Value.Y;
                    // cap to parent element bounds
                    currentPosition.X = Math.Max(0, Math.Min(NodeContainer.ActualWidth - item.Key.ActualWidth, currentPosition.X));
                    currentPosition.Y = Math.Max(0, Math.Min(NodeContainer.ActualHeight - item.Key.ActualHeight, currentPosition.Y));
                    // snap to grid (10x10 based on parent element offset)
                    currentPosition.X = (int)(currentPosition.X / gridSize) * gridSize;
                    currentPosition.Y = (int)(currentPosition.Y / gridSize) * gridSize;
                    // 요소 이동
                    Canvas.SetLeft(item.Key, currentPosition.X);
                    Canvas.SetTop(item.Key, currentPosition.Y);
                    // 스냅 커서 이동
                    SnapCursor.SetPosition(currentPosition.X, currentPosition.Y);
                }
            }
            else if (isResizing)
            {
                Point currentMousePosition = e.GetPosition(ZoomChild);
                int deltaX = (int)(currentMousePosition.X - lastMousePosition.X);
                int deltaY = (int)(currentMousePosition.Y - lastMousePosition.Y);
                foreach (var item in lastMouseOffsets)
                {
                    var lastElementSize = item.Value;
                    // 요소 크기 조정
                    switch (lastResizeDirection)
                    {
                        case ResizeGripDirection.Right:
                            {
                                int width = (int)(lastElementSize.X + deltaX);
                                width = Math.Max(30, Math.Min((int)(FooterZoomGrid.Width - Canvas.GetLeft(item.Key)), width));
                                item.Key.Width = JassProjectSpecification.Snap(width);
                                break;
                            }
                        case ResizeGripDirection.Bottom:
                            {
                                int height = (int)(lastElementSize.Y + deltaY);
                                height = Math.Max(30, Math.Min((int)(FooterZoomGrid.Height - Canvas.GetTop(item.Key)), height));
                                item.Key.Height = JassProjectSpecification.Snap(height);
                                break;
                            }
                        case ResizeGripDirection.BottomRight:
                            {
                                int width = (int)(lastElementSize.X + deltaX);
                                width = Math.Max(30, Math.Min((int)(FooterZoomGrid.Width - Canvas.GetLeft(item.Key)), width));
                                int height = (int)(lastElementSize.Y + deltaY);
                                height = Math.Max(30, Math.Min((int)(FooterZoomGrid.Height - Canvas.GetTop(item.Key)), height));
                                item.Key.Width = JassProjectSpecification.Snap(width);
                                item.Key.Height = JassProjectSpecification.Snap(height);
                            }
                            break;
                    }
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(ZoomChild);
                selectionRectPoint2X = mousePosition.X;
                selectionRectPoint2Y = mousePosition.Y;
                double left = Math.Min(selectionRectPoint1X, selectionRectPoint2X);
                double top = Math.Min(selectionRectPoint1Y, selectionRectPoint2Y);
                double width = Math.Abs(selectionRectPoint1X - selectionRectPoint2X);
                double height = Math.Abs(selectionRectPoint1Y - selectionRectPoint2Y);
                double zoomFactor = ((ScaleTransform)ZoomChild.LayoutTransform).ScaleX;
                Canvas.SetLeft(SelectionGrid, Canvas.GetLeft(ZoomChild) + left * zoomFactor);
                Canvas.SetTop(SelectionGrid, Canvas.GetTop(ZoomChild) + top * zoomFactor);
                SelectionGrid.Width = width * zoomFactor;
                SelectionGrid.Height = height * zoomFactor;
            }
            else if (selectedNodeIds.Count == 0)
            {
                Point mousePosition = e.GetPosition(ZoomChild);
                // 마우스 위치와 겹치는 TopMost 노드 선택
                ElemNode? topMostNode = NodeContainer.Children.OfType<ElemNode>()
                    .Where(node => new Rect(Canvas.GetLeft(node), Canvas.GetTop(node), node.ActualWidth, node.ActualHeight).Contains(mousePosition))
                    .OrderBy(node => Panel.GetZIndex(node))
                    .FirstOrDefault();
                if (topMostNode is null)
                {
                    Mouse.OverrideCursor = null;
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Hand;
                }
            }
            else
            {
                Point mousePosition = e.GetPosition(ZoomChild);
                // 마우스 위치와 겹치는 TopMost 노드 선택
                ElemNode? topMostNode = NodeContainer.Children.OfType<ElemNode>()
                    .Where(node => new Rect(Canvas.GetLeft(node), Canvas.GetTop(node), node.ActualWidth, node.ActualHeight).Contains(mousePosition))
                    .OrderBy(node => Panel.GetZIndex(node))
                    .FirstOrDefault();
                if (topMostNode is null)
                {
                    Mouse.OverrideCursor = null;
                }
                else
                {
                    var resizeDirection = UIResizeHelper.GetResizeDirection(topMostNode, e.GetPosition(topMostNode));
                    Mouse.OverrideCursor = resizeDirection switch
                    {
                        ResizeGripDirection.Right => Cursors.SizeWE,
                        ResizeGripDirection.Bottom => Cursors.SizeNS,
                        ResizeGripDirection.BottomRight => Cursors.SizeNWSE,
                        _ => Cursors.Hand,
                    };
                }
            }
        }

        private void OnGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 줌 적용 전, ZoomParent 좌표계에서 ZoomChild 내부에서의 좌표 (마우스 커서 위치 기준)
            Point cursorPositionInZoomParent = e.GetPosition(ZoomParent);

            // 줌 인/아웃에 따른 줌 팩터 적용
            double previousZoomFactor = ((ScaleTransform)ZoomChild.LayoutTransform).ScaleX;
            double nextZoomFactor = e.Delta > 0 ? previousZoomFactor * 1.1 : previousZoomFactor / 1.1;
            nextZoomFactor = Math.Max(0.1, Math.Min(2, nextZoomFactor));

            // 줌 적용
            var scaleTransform = (ScaleTransform)ZoomChild.LayoutTransform;
            scaleTransform.ScaleX = nextZoomFactor;
            scaleTransform.ScaleY = nextZoomFactor;

            // 현재 자식 정사각형에서 마우스의 상대적 위치 계산
            double relativeMousePosX = cursorPositionInZoomParent.X - Canvas.GetLeft(ZoomChild);
            double relativeMousePosY = cursorPositionInZoomParent.Y - Canvas.GetTop(ZoomChild);

            // 확대/축소 비율 계산 (X축과 Y축 각각)
            double scaleX = (ZoomChild.ActualWidth * nextZoomFactor) / (ZoomChild.ActualWidth * previousZoomFactor);
            double scaleY = (ZoomChild.ActualHeight * nextZoomFactor) / (ZoomChild.ActualHeight * previousZoomFactor);

            // 새로운 좌상단 좌표를 계산
            double newChildPosX = cursorPositionInZoomParent.X - (relativeMousePosX * scaleX);
            double newChildPosY = cursorPositionInZoomParent.Y - (relativeMousePosY * scaleY);

            Canvas.SetLeft(ZoomChild, newChildPosX);
            Canvas.SetTop(ZoomChild, newChildPosY);

            MessageText.Info($"확대/축소 {(int)(nextZoomFactor * 100)}%");
        }

        private void SelectionAdd(List<long> nodeIds)
        {
            nodeIds.ForEach(nodeId => SelectionAdd(nodeId));
        }

        private void SelectionAdd(long nodeHandleId)
        {
            if (selectedNodeIds.ContainsKey(nodeHandleId)) return;

            // 논리 선택 갱신
            selectedNodeIds.Add(nodeHandleId, true);
            // 비주얼 선택
            nodeElements[nodeHandleId].MarkAsSelected();
        }

        private void SelectionClear()
        {
            // 비주얼 선택 해제
            selectedNodeIds.Keys.Select(nodeHandleId => nodeElements[nodeHandleId]).ToList().ForEach(node => node.MarkAsUnselected());
            // 논리 선택 초기화
            selectedNodeIds.Clear();
        }
    }
}
