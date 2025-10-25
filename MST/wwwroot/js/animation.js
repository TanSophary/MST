//animation.js
// Scroll-triggered animations for all elements with 'animate' class
document.addEventListener('DOMContentLoaded', () => {
    const elements = document.querySelectorAll('.animate');

    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target); // Stop observing once animated
            }
        });
    }, observerOptions);

    elements.forEach(element => {
        observer.observe(element);
    });
});