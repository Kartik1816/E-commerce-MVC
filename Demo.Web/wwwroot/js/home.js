
const words = ["Computers", "Laptops", "Accessories"];
let wordIndex = 0;
let charIndex = 0;
const typingSpeed = 150; // Speed of typing each character
const erasingSpeed = 100; // Speed of erasing each character
const delayBetweenWords = 1500; // Delay before typing the next word

function typeWriterEffect() {
    const span = document.getElementById("typewriter");

    if (charIndex < words[wordIndex].length) {
        // Add one character at a time
        span.textContent += words[wordIndex][charIndex];
        charIndex++;
        setTimeout(typeWriterEffect, typingSpeed);
    } else {
        // Wait before erasing
        setTimeout(() => eraseEffect(span), delayBetweenWords);
    }
}

function eraseEffect(span) {
    if (charIndex > 0) {
        // Remove one character at a time
        span.textContent = span.textContent.slice(0, -1);
        charIndex--;
        setTimeout(() => eraseEffect(span), erasingSpeed);
    } else {
        // Move to the next word
        wordIndex = (wordIndex + 1) % words.length; // Cycle through words
        setTimeout(typeWriterEffect, typingSpeed);
    }
}

// Start the typewriter effect
typeWriterEffect();



function getProductsByCategory(categoryId)
{
    window.location.href = `/CLA/${categoryId}`;
}