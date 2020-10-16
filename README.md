# Copy Git Link

Copy links to files or selections to GitHub and Azure DevOps directly from Visual Studio's text editor, Solution Explorer and document tab.
Useful to share a link to a teammate without leaving the IDE.

Download from the Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=EtienneBAUDOUX.CopyGitLink

# Copy Link to Selection

![image__1.png](https://etiennebaudoux.gallerycdn.vsassets.io/extensions/etiennebaudoux/copygitlink/1.0/1602871432029/image__1.png)

# Copy Link to Method, Type, Property using CodeLens

![image__2.png](https://etiennebaudoux.gallerycdn.vsassets.io/extensions/etiennebaudoux/copygitlink/1.0/1602871432029/image__2.png)

![image__3.png](https://etiennebaudoux.gallerycdn.vsassets.io/extensions/etiennebaudoux/copygitlink/1.0/1602871432029/image__3.png)

# Copy Link to File from document tab

![image__4.png](https://etiennebaudoux.gallerycdn.vsassets.io/extensions/etiennebaudoux/copygitlink/1.0/1602871432029/image__4.png)

# Copy Link to File from Solution Explorer

## Supports
* Solution View
* Folder View

![image__6.png](https://etiennebaudoux.gallerycdn.vsassets.io/extensions/etiennebaudoux/copygitlink/1.0/1602871432029/image__6.png)

# FAQ

**Q:** How do I use it?

**A:** Install the extension, open in Visual Studio a local file coming from any Azure DevOps or GitHub repository. No need to open an entire solution, just a document is enough. Then, use one of the menu showed above to copy a link.


**Q:** Can I share a link from any branch?

**A:** Yes, as long as what you're trying to share exists online (so make sure the code or file you share has been pushed on the current branch).


**Q:** What if I want to share a local change?

**A:** You need to Push the change first.


**Q:** What if I share a code existing in, let’s say, Master branch on the remote repository, but that I’m on a local branch that isn’t pushed yet?

**A:** If your branch doesn’t exist online, it will generate a link using the default branch on the remote repository (commonly Master).
    But of course if your local branch differs too much from the default remote one, the link may be inaccurate.
    
**Q:** Does it works from a Live Share guest session, or GitHub Codespaces session?
**A:** No.
