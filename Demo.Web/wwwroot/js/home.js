

// const words = ["Computers", "Laptops", "Accessories"];
// let wordIndex = 0;
// let charIndex = 0;
// const typingSpeed = 150; // Speed of typing each character
// const erasingSpeed = 100; // Speed of erasing each character
// const delayBetweenWords = 1500; // Delay before typing the next word

// function typeWriterEffect() {
//     const span = document.getElementById("typewriter");

//     if (charIndex < words[wordIndex].length) {
//         // Add one character at a time
//         span.textContent += words[wordIndex][charIndex];
//         charIndex++;
//         setTimeout(typeWriterEffect, typingSpeed);
//     } else {
//         // Wait before erasing
//         setTimeout(() => eraseEffect(span), delayBetweenWords);
//     }
// }

// function eraseEffect(span) {
//     if (charIndex > 0) {
//         // Remove one character at a time
//         span.textContent = span.textContent.slice(0, -1);
//         charIndex--;
//         setTimeout(() => eraseEffect(span), erasingSpeed);
//     } else {
//         // Move to the next word
//         wordIndex = (wordIndex + 1) % words.length; // Cycle through words
//         setTimeout(typeWriterEffect, typingSpeed);
//     }
// }

// // Start the typewriter effect
// typeWriterEffect();



function getProductsByCategory(categoryId)
{
    $.ajax({
        type: 'GET',
        url: '/CLA/GetEncryptedId',
        data : { Id: categoryId },
        success: function (data) {
            if (data.success) {
                window.location.href = `/CLA/${data.encryptedId}`;
            } else {
                toastr.error(data.message);
            }
        },
    });
}


$(document).ready(function () {
    $('.owl-carousel').owlCarousel({
        items: 1, // Show one item at a time
        loop: true,
        // nav: true, // Next/Prev arrows
        dots: true, // Dots navigation
        autoplay: true,
        autoplayTimeout: 3000, // 3000ms = 3s delay
        autoplayHoverPause: true, // Pause on hover
        // navText: ["<", ">"], // You can customize the arrow text/icons
    });

});

function onCategoryCheckboxChange() {
    const selectedIds = $('.category-checkbox:checked').map(function () {
        return parseInt($(this).val());
    }).get();

    $.ajax({
        url: '/Home/FilterCategories',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (result) {
            $('#categoryContainer').html(result);
        },
        error: function (xhr, status, error) {
            console.error("Error filtering categories:", error);
        }
    });
}
   