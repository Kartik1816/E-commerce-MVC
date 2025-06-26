const image = document.getElementById('productImage');
    const lens = document.getElementById('zoomLens');
    const zoomResult = document.createElement('div');
    zoomResult.className = 'zoom-result';
    document.body.appendChild(zoomResult);

    const zoomedImage = document.createElement('img');
    zoomedImage.src = image.src;
    zoomResult.appendChild(zoomedImage);

    image.addEventListener('mouseenter', () => {
        lens.style.display = 'block';
    });
    
    // Hide both when mouse leaves image
    image.addEventListener('mouseleave', () => {
        lens.style.display = 'none';
        zoomResult.style.display = 'none';
    });

    lens.addEventListener('mousemove', moveLens);
    image.addEventListener('mousemove', moveLens);
    lens.addEventListener('mouseleave', () => {
        zoomResult.style.display = 'none';
    });
    image.addEventListener('mouseleave', () => {
        zoomResult.style.display = 'none';
    });

    function moveLens(e) {
        //Get the bounding rectangle of the image
        const rect = image.getBoundingClientRect();

        //Calculate the cursor's position relative to the image
        //e.clientX and e.clientY represent the cursor's position relative to the viewport.
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;


        //Calculate the lens's center position
        //lens.offsetWidth and lens.offsetHeight represent the dimensions of the zoom lens.
        //Dividing by 2 gives the half-width and half-height of the lens.
        const lensWidth = lens.offsetWidth / 2;
        const lensHeight = lens.offsetHeight / 2;

        let lensX = x - lensWidth;
        let lensY = y - lensHeight;

        // Prevent lens from going outside the image
        lensX = Math.max(0, Math.min(lensX, rect.width - lens.offsetWidth));
        lensY = Math.max(0, Math.min(lensY, rect.height - lens.offsetHeight));


        //Update the lens's position
        //The lens's position is updated using style.left and style.top to match the calculated lensX and lensY.
        lens.style.left = `${lensX}px`;
        lens.style.top = `${lensY}px`;


        //Display the zoom result container
        //zoomResult.style.display = 'block' ensures the zoomed image is visible.
        //zoomResult.style.left and zoomResult.style.top position the zoom result container to the right of the image (rect.right + 10) and aligned vertically with the image (rect.top).

        zoomResult.style.display = 'block';
        zoomResult.style.left = `${rect.right + 10}px`;
        zoomResult.style.top = `${rect.top}px`;


        //Calculate the zoomed image's position
        //zoomX and zoomY calculate the position of the zoomed image relative to the lens
        const zoomX = lensX / rect.width * zoomedImage.offsetWidth;
        const zoomY = lensY / rect.height * zoomedImage.offsetHeight;

        zoomedImage.style.left = `-${zoomX}px`;
        zoomedImage.style.top = `-${zoomY}px`;
    }