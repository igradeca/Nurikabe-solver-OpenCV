# Nurikabe-solver-OpenCV

Project consists two parts:
- Nurikabe solver with heuristic method
- Nurikabe grid detection from camera input

### Nurikabe solver with heuristic method

In this project I have made Nurikabe solver with heuristic method only following tutorials on how to solve Nurikabe (as human). 
Algorithm is solving Nurikabe with common rules in a while loop until no new cell is changed in comparison on previous step.
After that Algorithm adds random new cell (black/full) at random location and checks if everything is in order by the Nurikabe rules.
Then again heuristic algorithm is staring and trying to solve it. This process is working until the whole grid is not solved. 
Possible errors in solving are eliminated by forking solving cases in binary-tree-like scenario nodes where algorithm can return to previous 
state if it reaches dead-end scenario.

### Nurikabe grid detection from camera input

Grid dectection works in a way that program is taking each camera frame and turning it into black and white image. After that we are 
applying Median smoothing and adaptive Gauss filter to get binary image. After than we apply Median once again to remove possible noise. 
To make grid lines more detectable we apply morphological operation Dilation to make them thicker and with Canny edge detector we separete
cells to be more easily to detect. After that we extract grid lines with Hough transformation. Points where lines are crossing we save as
places where grid cells should be.  

Taking 4 points we get one cell location from where we extract number of empty cell. Numbers are detected with machine learning technique 
SVM (Support vector machine) and then sent to Nurikabe solver. After the solving we simply paint cells in camera frame according to solution.
  
![alt text](https://github.com/igradeca/Nurikabe-solver-OpenCV/blob/master/sl1.png)
![alt text](https://github.com/igradeca/Nurikabe-solver-OpenCV/blob/master/sl4.png)
![alt text](https://github.com/igradeca/Nurikabe-solver-OpenCV/blob/master/sl2.png)
![alt text](https://github.com/igradeca/Nurikabe-solver-OpenCV/blob/master/sl3.png)

## References

1. Wicht B., Hennebert J., (2014). Camera-based Sudoku recognition with Deep Belief Network. SoCPaR 2014  
2. Hữu Quân C., (2017). Building a simple SUDOKU Solver from scratch - Part 1: Grid Detection & Digit Extraction. https://caphuuquan.blogspot.com/2017/04/building-simple-sudoku-solver-from.html  
3. Hữu Quân C., (2017). How to detect lines in image using Hough Line Transform (with code example). https://caphuuquan.blogspot.com/2017/03/how-to-detect-lines-in-image-using.html  
4. Hữu Quân C., (2017). Building a simple SUDOKU Solver from scratch - Part 2: Digit Number Recognition using SVM. https://caphuuquan.blogspot.com/2017/04/building-simple-sudoku-solver-from_23.html  
5. Pont-Tuset J., (2018). Solving Sudoku puzzles like a pro (part I). http://jponttuset.cat/solving-sudokus-like-a-pro-1/  
6. Pont-Tuset J., (2018). Solving Sudoku puzzles like a pro (part II). http://jponttuset.cat/solving-sudokus-like-a-pro-2/  
7. Tech AJ, (2017). Digit Recognition with SVM in Emgu CV 3.3. https://github.com/halanch599/Emgucv/tree/master/Digit%20CLassification%20SVM  
8. Diligent Key Presser, (2014). Converting jagged array to 2D array C#. https://stackoverflow.com/questions/26291609/converting-jagged-array-to-2d-array-c-sharp  
9. OpenCV, (2017). Image Thresholding. https://docs.opencv.org/3.4.0/d7/d4d/tutorial_py_thresholding.html  
10. OpenCV, (2014). Morphological Transformations. https://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_morphological_ops/py_morphological_ops.html  
