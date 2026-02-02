**Project Request: PDF Merger Desktop Application**

Please build a desktop application with the following specifications:

**Technology Stack:**
- .NET 10
- Windows Desktop application (WPF or WinForms)
- PdfSharp library for PDF operations

**Core Functionality:**
Create a PDF merger application that allows users to combine multiple files into a single PDF document.

**Supported File Types:**
- Image files (PNG, JPG, JPEG, BMP, GIF, TIFF)
- PDF files

**User Interface Requirements:**
1. A drag-and-drop zone where users can add files
2. A list/grid view showing all added files in their current order
3. Ability to reorder files (move up/down or drag to reorder)
4. Ability to remove individual files from the list
5. A "Merge" or "Create PDF" button to execute the merge
6. A file browser button as an alternative to drag-and-drop
7. Progress indicator during merge operation
8. Success/error notifications

**Technical Requirements:**
1. Use PdfSharp library for all PDF operations
2. Preserve the exact order of files as arranged by the user
3. Convert image files to PDF pages before merging
4. Handle errors gracefully (invalid files, crashes on pdf creation, etc.)
5. Allow users to select output location and filename for the merged PDF

**Additional Considerations:**
- Ensure PdfSharp is properly installed or bundled with the application
- Validate file types before adding to the list
- Provide clear user feedback throughout the process
- Make the UI intuitive and user-friendly

Please provide the complete solution including all necessary code files, project structure, and instructions for setup and dependencies.