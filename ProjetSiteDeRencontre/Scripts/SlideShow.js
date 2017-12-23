var slideIndex = 1;

function plusSlides(n) {
    showSlides(slideIndex += n);
}
function currentSlide(n) {
    showSlides(slideIndex = n);
}

function showSlides(n) {
    var i;
    var slides = document.getElementsByClassName("mySlides");
    var photosDetail = document.getElementsByClassName("photoDetail");
    var photos = document.getElementsByClassName("photoDetailLaPhoto");
    //var dots = document.getElementsByClassName("dot");

    if (n > slides.length) { slideIndex = 1 }
    if (n < 1) { slideIndex = slides.length }

    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
        photos[i].classList.remove("photoSelectionee");

        slides[i].classList.remove("prevPhotoCarousel");
        slides[i].classList.remove("nextPhotoCarousel");

        photos[i].classList.remove("nextPrevPhoto");
        photosDetail[i].classList.remove("nextPrevPhoto");
    }
    //for(i = 0; i < dots.length; i++)
    //{
    //    dots[i].className = dots[i].className.replace(" active", "");
    //}

    slides[slideIndex - 1].style.display = "block";
    photos[slideIndex - 1].classList.add("photoSelectionee");

    if (slides.length >= 2) {
        var previousSlideIndex = slideIndex - 1;
        if (previousSlideIndex > slides.length) { previousSlideIndex = 1 }
        if (previousSlideIndex < 1) { previousSlideIndex = slides.length }

        slides[previousSlideIndex - 1].style.display = "block";
        slides[previousSlideIndex - 1].classList.add("prevPhotoCarousel");

        photos[previousSlideIndex - 1].classList.add("nextPrevPhoto");

        photosDetail[previousSlideIndex - 1].classList.add("nextPrevPhoto");

        //dots[slideIndex-1].className += " active";
    }
    if (slides.length >= 3) {
        var nextSlideIndex = slideIndex + 1;
        if (nextSlideIndex > slides.length) { nextSlideIndex = 1 }
        if (nextSlideIndex < 1) { nextSlideIndex = slides.length }

        slides[nextSlideIndex - 1].style.display = "block";
        slides[nextSlideIndex - 1].classList.add("nextPhotoCarousel");

        photos[nextSlideIndex - 1].classList.add("nextPrevPhoto");

        photosDetail[nextSlideIndex - 1].classList.add("nextPrevPhoto");
    }
}