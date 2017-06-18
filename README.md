
OrgaSnismo0, is based in the game of life, designed by the Mathematician John Conway. There´s an extensive information in Wikipedia.

**Basic operation**

When the program starts, apart of a menu bar which is commented later, the button 'Draw initial shape' appears.

![image-1](https://github.com/Xadnem53/OrgaSnismo0/blob/master/Images/Initial.png)


After do click on this button, the following elements are shown:

**Grid:** A new yellow cell is created by clicking over a square. Clicking over a 	previously created cell, erase it

**'Manual cycle' check button:** It is checked by defect, and is used to stablish  the cycle time by the user.
                                                When is unchecked, the check button and the spin-button change theirs colours and the 			      cycle time is established automatically function the time taken by the CPU to calculate 			      the next shape, i.e. as fast as the CPU can process the algorithm.

**Spin button:**  Shows the established cycle time. It is set at 100 milliseconds by defect.
                      The cycle time can be changed when 'manual cycle' is checked.
                      When the time that the CPU take to calculate a new shape is bigger than the cycle time settled down, the 	        program change to auto by itself.

**'Accept' button:** To start the cycles once the desired shaped has been drawn or loaded and the cycle time established.

![image-2](https://github.com/Xadnem53/OrgaSnismo0/blob/master/Images/drawshape.png)


Once the game is started, the drawn shape is going changing according to the rules at every new cycle.
By defect, the rules are the ones established by Conway. i.e.:
A cell remain alive if it is adjacent to two alive cells, otherwise, the cell dies at next cycle.
A cell born, if it is adjacent to three alive cells.

At the window bottom left, the population ( alive cells account ) and the cycles passed is shown.
Also the : zoom , displacement and pause, buttons appear.

![image-3](https://github.com/Xadnem53/OrgaSnismo0/blob/master/Images/running.png)

If the shape stabilizes or disappears, a message box is shown and the game is over.




**Menu bar**

**File:**

**New:** To start a new game.

**Save:** Save the current shape, rules and colours in a *.fm file. One can save a 	shape, either that the cycles have started
          or not. If the cycles are running, it is convenient to do the file saving process with the game paused.

**Load:** Load a shape with the rules and colour previously saved in a *.fm file.

**Save image:** Save a *.png image of the current screen. The png file, is saved in	a directory called 'imagenes' which is 	       created , if it doesn´t exist, into the directory where the application executable is.

**Options:**							

**Change rules:** A window appears an allow to change the rules. The rules will remain if a new game is started.
	          At the change rules window bottom, there are some rules examples with a brief description of their 		          respective effects.





















**Change colors:** The 5 buttons shown, control respectively the following colours:
		   The perimeter cell line ( red by defect ),
		   the alive cell interior ( yellow by defect ),
		   the new cells born interior ( yellow by defect ) 
		   the died cells interior ( black by defect ) and 
		   the background ( black by defect ).



**Back to values by defect:** Return to the rules and colours by defect .

**Autosave images:** Save a *.png image of the new shape each cycle.
		  The png images are saved at the directory called 'imagenes', which is created if it doesn´t exist, into 		  the directory where the application executable is.
                 	  The file name of each png saved image, contains the population and cycles information.

**Language:** Spanish and English languages are available.




