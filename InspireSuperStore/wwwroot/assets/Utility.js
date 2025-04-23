const Utility = {
















    /**
     * Validate a form and all its inputs.
     * @param {string} formSelector - The CSS selector for the form to validate.
     */
    validateFormAndInputs: function (formSelector) {
        const form = document.querySelector(formSelector);
        if (!form) {
            console.error('Form not found:', formSelector);
            return false; // Return false if the form is not found
        }
        debugger

        // Validate inputs on input event (real-time feedback)
        form.querySelectorAll('input, select, textarea').forEach(input => {
            input.addEventListener('input', function () {
                if (input.checkValidity()) {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                } else {
                    input.classList.remove('is-valid');
                    input.classList.add('is-invalid');
                }
            });
        });

        // Validate form on submit event
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                form.classList.add('was-validated');
                return false; // Prevent form submission if invalid
            }

            form.classList.add('was-validated');
            return true; // Allow form submission if valid
        });

        // Function to manually trigger validation and return result
        return function validate() {
            const allValid = [...form.querySelectorAll('input, select, textarea')].every(input => input.checkValidity());

            // Add validation classes to inputs
            form.querySelectorAll('input, select, textarea').forEach(input => {
                if (input.checkValidity()) {
                    input.classList.remove('is-invalid');
                    input.classList.add('is-valid');
                } else {
                    input.classList.remove('is-valid');
                    input.classList.add('is-invalid');
                }
            });

            return allValid;
        };
    },
  SpinnerMessage: function(brothel, message) {
    debugger;
    const targetElement = document.getElementById(brothel);

    if (targetElement) {
        // Ensure the target element is relatively positioned for overlay
        targetElement.style.position = 'relative';

        // Create the overlay container
        const overlay = document.createElement('div');
        overlay.style.position = 'absolute';
        overlay.style.top = '0';
        overlay.style.left = '0';
        overlay.style.width = '100%';
        overlay.style.height = '100%';
        overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.4)'; // Dark gray with 40% transparency
        overlay.style.display = 'flex';
        overlay.style.flexDirection = 'column'; // Arrange spinner and message vertically
        overlay.style.alignItems = 'center';
        overlay.style.justifyContent = 'center';
        overlay.style.zIndex = '9999'; // Ensure it overlays on top

        // Create the spinner element
        const spinner = document.createElement('i');
        spinner.className = 'icon-spinner6 spinner'; // Add spinner classes
        spinner.style.fontSize = '24px'; // Optional: Adjust spinner size

        // Create the message element
        const messageElement = document.createElement('div');
        messageElement.textContent = message || 'Loading...'; // Use custom message or default
        messageElement.style.marginTop = '10px'; // Space between spinner and message
        messageElement.style.color = '#fff'; // White text for visibility
        messageElement.style.fontSize = '16px'; // Optional: Adjust text size

        // Append spinner and message to the overlay
        overlay.appendChild(spinner);
        overlay.appendChild(messageElement);

        // Append the overlay to the target element
        targetElement.appendChild(overlay);
    } else {
        console.error(`Element with ID "${brothel}" not found.`);
    }
},
StopSpinnerMessage: function(brothel) {
    debugger;
    const targetElement = document.getElementById(brothel);

    if (targetElement) {
        // Find the overlay within the target element
        const overlay = targetElement.querySelector('div[style*="position: absolute"]');

        if (overlay) {
            // Remove the overlay
            targetElement.removeChild(overlay);
        } else {
            console.error('No overlay found to remove.');
        }
    } else {
        console.error(`Element with ID "${brothel}" not found.`);
    }
}
,

SpinnerMessageParent: function(brothel, message) {
    debugger;
    const targetElement = document.getElementById(brothel);

    if (targetElement) {
        // Find the closest parent with class 'card'
        const parentCard = targetElement.closest('.card');

        if (parentCard) {
            // Ensure the parent element is relatively positioned for overlay
            parentCard.style.position = 'relative';

            // Create the overlay container
            const overlay = document.createElement('div');
            overlay.style.position = 'absolute';
            overlay.style.top = '0';
            overlay.style.left = '0';
            overlay.style.width = '100%';
            overlay.style.height = '100%';
            overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.4)'; // Dark gray with 40% transparency
            overlay.style.display = 'flex';
            overlay.style.flexDirection = 'column'; // Arrange spinner and message vertically
            overlay.style.alignItems = 'center';
            overlay.style.justifyContent = 'center';
            overlay.style.zIndex = '9999'; // Ensure it overlays on top

            // Create the spinner element
            const spinner = document.createElement('i');
            spinner.className = 'icon-spinner6 spinner'; // Add spinner classes
            spinner.style.fontSize = '24px'; // Optional: Adjust spinner size

            // Create the message element
            const messageElement = document.createElement('div');
            messageElement.textContent = message || 'Loading...'; // Use custom message or default
            messageElement.style.marginTop = '10px'; // Space between spinner and message
            messageElement.style.color = '#fff'; // White text for visibility
            messageElement.style.fontSize = '16px'; // Optional: Adjust text size

            // Append spinner and message to the overlay
            overlay.appendChild(spinner);
            overlay.appendChild(messageElement);

            // Append the overlay to the parent card
            parentCard.appendChild(overlay);
        } else {
            console.error(`Parent element with class "card" not found for ID "${brothel}".`);
        }
    } else {
        console.error(`Element with ID "${brothel}" not found.`);
    }
},
StopSpinnerMessageParent: function(brothel) {
    debugger;
    const targetElement = document.getElementById(brothel);

    if (targetElement) {
        // Find the closest parent with class 'card'
        const parentCard = targetElement.closest('.card');

        if (parentCard) {
            // Find the overlay within the parent card
            const overlay = parentCard.querySelector('div[style*="position: absolute"]');

            if (overlay) {
                // Remove the overlay
                parentCard.removeChild(overlay);
            } else {
                console.error('No overlay found to remove.');
            }
        } else {
            console.error(`Parent element with class "card" not found for ID "${brothel}".`);
        }
    } else {
        console.error(`Element with ID "${brothel}" not found.`);
    }
},


    /**
  * Display a notification.
  * @param {string} message - The message to display in the notification.
  */
    successMessage: function (message) {
        new Noty({
            theme: ' alert alert-success alert-styled-left p-0 bg-white',
            text: message,
            type: 'success',
            progressBar: false,
            closeWith: ['button']
        }).show();
    },
    failMessage: function (message) {
        new Noty({
            theme: ' alert alert-danger alert-styled-left p-0 bg-white',
            text: message,
            type: 'success',
            progressBar: false,
            closeWith: ['button']
        }).show();
    },
    infoMessage: function (message) {
        new Noty({
            theme: ' alert alert-primary alert-styled-left p-0 bg-white',
            text: message,
            type: 'success',
            progressBar: false,
            closeWith: ['button']
        }).show();
    },
   
    validateNumericValue: function validateFloatInput(input, decimalPlaces) {
        let value = input.value.trim(); // Get the input value and trim whitespace
        debugger
        // Remove non-numeric characters except for `-` and `.`
        value = value.replace(/[^0-9.-]/g, '');

        // Parse the value to a float
        let floatValue = parseFloat(value);

        if (!isNaN(floatValue)) {
            // Format the value based on the decimalPlaces parameter
            if (decimalPlaces >= 0) {
                input.value = floatValue.toFixed(decimalPlaces);
            } else {
                input.value = floatValue.toString(); // Default behavior if decimalPlaces is not provided
            }
        } else {
            // If the value is not a valid float, clear the input
            input.value = '';
        }
    },
    validateAlphaNumeric: function validateAlphaNumericInput(input) {
        // Remove all characters except alphanumeric, dash (`-`), and dot (`.`)
        input.value = input.value.replace(/[^a-zA-Z0-9.-]/g, '');
    },
    startSpinner: function startSpinner(element, message) {
        var block = $(element); // Get the target element
        $(block).block({
            message: `<span class="font-weight-semibold"><i class="icon-spinner4 spinner mr-2"></i>&nbsp; ${message}</span>`,
            overlayCSS: {
                backgroundColor: '#fff',
                opacity: 0.8,
                cursor: 'wait'
            },
            css: {
                border: 0,
                padding: 0,
                backgroundColor: 'transparent'
            }
        });
    },
    stopSpinner: function stopSpinner(element) {
        var block = $(element); // Get the target element
        $(block).unblock(); // Unblock the element
    }
};
