# NeonRush
This is the official Unity client repository for **NeonRush**.  
We previously used UnitySCM for version control, but have migrated to Git.  
Note: The project is already in mid-development stage.
---

## 📦 How to restore External Assets

Some large files are excluded from this repository due to size limits.

They are available as a compressed ZIP archive via Google Drive.

### 🔗 Download

👉 [Google Drive – External Asset ZIP](https://drive.google.com/file/d/1ksPiVdGq1p2raVzW0y5ByHkP6ZlLGBdm/view?usp=sharing)
**Of course, permission needs to be aquired by our team to download this file**

### 📂 Installation Instructions

1. Download the ZIP file from the link above.
2. Extract the ZIP file.
3. Move the extracted folder into your Unity project at:

### Assets/External Asset/

4. ✅ Make sure the folder structure remains **exactly as it was extracted**. > ⚠️ Do **not** change the folder name (`External Asset`) or move files individually. > Unity references (prefabs, textures, audio) may break if the structure changes.

💡 Note ------- The `External Asset` folder and all its contents are excluded from version control via `.gitignore`. Please **do not commit or push** any of these files.