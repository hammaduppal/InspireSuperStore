const encryptionKey = "9AB9>'q}i(3|7hQ0Z*Ph0r0PF>U1QU";

// Generate a key using SHA-256
function generateKey(key) {
    const hash = CryptoJS.SHA256(key);
    return CryptoJS.enc.Hex.parse(hash.toString()); // 32-byte key for AES-256
}
function hashPassword(password) {
    return CryptoJS.SHA256(password).toString(CryptoJS.enc.Hex);
}
// Encrypt a plaintext string


function encryptwithHash(plainText) {
    if (!plainText) {
        return '';
    }

    const hashedPlainText = hashPassword(plainText);
    const iv = CryptoJS.lib.WordArray.random(16);
    const key = generateKey(encryptionKey);

    const encrypted = CryptoJS.AES.encrypt(hashedPlainText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    const encryptedData = iv.concat(encrypted.ciphertext);

    return CryptoJS.enc.Base64.stringify(encryptedData);

}


function encrypt(plainText) {
    if (!plainText) {
        return '';
    }

    // Generate a random 16-byte IV
    const iv = CryptoJS.lib.WordArray.random(16);
    const key = generateKey(encryptionKey);

    // Encrypt the plaintext
    const encrypted = CryptoJS.AES.encrypt(plainText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    // Combine IV and ciphertext (prepend IV to the ciphertext)
    const encryptedData = iv.concat(encrypted.ciphertext);

    // Return base64-encoded result
    return CryptoJS.enc.Base64.stringify(encryptedData);
}



// Decrypt an encrypted string
function decrypt(cipherText) {
    if (!cipherText) {
        return '';
    }

    // Decode the Base64 input
    const cipherBytes = CryptoJS.enc.Base64.parse(cipherText);

    // Extract the IV (first 16 bytes)
    const iv = CryptoJS.lib.WordArray.create(cipherBytes.words.slice(0, 4), 16);

    // Extract the ciphertext (remaining bytes)
    const encryptedData = CryptoJS.lib.WordArray.create(cipherBytes.words.slice(4));

    // Generate the key
    const key = generateKey(encryptionKey);

    // Decrypt the data
    const decrypted = CryptoJS.AES.decrypt(
        { ciphertext: encryptedData },
        key,
        {
            iv: iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        }
    );

    // Convert decrypted data to a string
    return decrypted.toString(CryptoJS.enc.Utf8);
}


var idleTime = 0;
$(document).ready(function () {
    $(document).on('mousemove keydown scroll', function () {
        idleTime = 0;
    });
    var idleInterval = setInterval(async function () {
        idleTime++;
        if (idleTime >= 200) {
            clearInterval(idleInterval);
            $.get('/Account/LogOut', function (response) {
            }).fail(function () {
            });
            await bootboxAlertAsync("Account Signed Out!", "Your application was found idle and was signed out automatically to prevent unauthorized access.<br/><br/>Please login again to continue.");
            var pathQuery = window.location.pathname + window.location.search;
            clearInterval(idleInterval);
            window.location.href = '/Account/LogOut?returnUrl=' + encodeURIComponent(pathQuery);
        }
    }, 200);


});


function bootboxAlertAsync(title, message) {
    return new Promise((resolve) => {
        bootbox.alert({
            title: title,
            message: message,
            callback: function () {
                resolve();
            }
        });
    });
}




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
    SpinnerMessage: function (brothel, message) {
        
        const targetElement = document.getElementById(brothel);
        debugger
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

    StopSpinnerMessage: function (brothel) {
        ;
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

    SpinnerMessageParent: function (brothel, message) {
        ;
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
    StopSpinnerMessageParent: function (brothel) {
        ;
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
        debugger
        let value = input.value.trim(); 
        
        value = value.replace(/[^0-9.-]/g, '');

        let floatValue = parseFloat(value);

        if (!isNaN(floatValue)) {
            if (decimalPlaces >= 0) {
                input.value = floatValue.toFixed(decimalPlaces);
            } else {
                input.value = floatValue.toString(); 
            }
        } else {
            input.value = 0.00;
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
    },
    RequestDataAjax: async function (data) {
        try {
            
            const response = await $.ajax({
                url: data.url,
                data: data.data,
                type: 'POST'
            });
            return response;
        } catch (error) {
            
            console.error("AJAX error:", error);
            return null;
        }
    },
    RequestAjax: async function (url) {
        try {
            const response = await $.ajax({
                url: url,
                type: 'POST'
            });
            return response;
        } catch (error) {
            console.error("AJAX error:", error);
            return null;
        }
    }

};
