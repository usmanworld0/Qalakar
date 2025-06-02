// Import the functions you need from the SDKs you need
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js";
import { getAuth, createUserWithEmailAndPassword, signInWithEmailAndPassword, signOut, sendEmailVerification, onAuthStateChanged } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-auth.js";
import { getFirestore, collection, addDoc, getDocs, doc, getDoc, updateDoc, deleteDoc, query, where, orderBy, serverTimestamp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js";

// Your Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyBJVfLcjVgBQ6JnVVwvBdG9UFWX3-otvbw",
    authDomain: "qalakar-9c573.firebaseapp.com",
    projectId: "qalakar-9c573",
    storageBucket: "qalakar-9c573.firebasestorage.app",
    messagingSenderId: "1084126615084",
    appId: "1:1084126615084:web:c803167b354e7d52ac2389",
    measurementId: "G-D68D1PK8Z7"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
const db = getFirestore(app);

// Helper function to get user-friendly error messages
function getErrorMessage(errorCode) {
    switch (errorCode) {
        case 'auth/user-not-found':
            return 'No account found with this email address.';
        case 'auth/wrong-password':
            return 'Incorrect password. Please try again.';
        case 'auth/email-already-in-use':
            return 'An account with this email already exists.';
        case 'auth/weak-password':
            return 'Password should be at least 6 characters long.';
        case 'auth/invalid-email':
            return 'Please enter a valid email address.';
        case 'auth/too-many-requests':
            return 'Too many failed attempts. Please try again later.';
        case 'auth/network-request-failed':
            return 'Network error. Please check your connection and try again.';
        default:
            return 'An error occurred. Please try again.';
    }
}

// Monitor authentication state changes
onAuthStateChanged(auth, (user) => {
    // Immediate notification to Blazor
    if (window.blazorAuthStateProvider) {
        try {
            window.blazorAuthStateProvider.invokeMethodAsync('OnFirebaseAuthStateChanged');
            
            // Additional notifications at shorter intervals
            setTimeout(() => {
                if (window.blazorAuthStateProvider) {
                    window.blazorAuthStateProvider.invokeMethodAsync('OnFirebaseAuthStateChanged');
                }
            }, 200);
            
            setTimeout(() => {
                if (window.blazorAuthStateProvider) {
                    window.blazorAuthStateProvider.invokeMethodAsync('OnFirebaseAuthStateChanged');
                }
            }, 500);
        } catch (error) {
            // Silent error handling
        }
    }
});

// Firebase Auth functions
window.firebaseAuth = {
    signUp: async (email, password) => {
        try {
            const userCredential = await createUserWithEmailAndPassword(auth, email, password);
            
            await sendEmailVerification(userCredential.user);
            
            return {
                success: true,
                message: "Account created successfully! Please check your email for verification."
            };
        } catch (error) {
            return {
                success: false,
                message: getErrorMessage(error.code)
            };
        }
    },

    signIn: async (email, password) => {
        try {
            const userCredential = await signInWithEmailAndPassword(auth, email, password);
            const user = userCredential.user;
            
            if (!user.emailVerified) {
                return {
                    success: false,
                    message: "Please verify your email before signing in. Check your inbox for a verification link."
                };
            }
            
            return {
                success: true,
                message: "Signed in successfully!",
                user: {
                    email: user.email,
                    uid: user.uid
                }
            };
        } catch (error) {
            return {
                success: false,
                message: getErrorMessage(error.code)
            };
        }
    },

    signOut: async () => {
        try {
            await signOut(auth);
            
            return {
                success: true,
                message: "Signed out successfully!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Error signing out: " + error.message
            };
        }
    },

    resendVerification: async () => {
        try {
            const user = auth.currentUser;
            if (user) {
                await sendEmailVerification(user);
                return {
                    success: true,
                    message: "Verification email sent! Check your inbox."
                };
            } else {
                return {
                    success: false,
                    message: "No user is currently signed in."
                };
            }
        } catch (error) {
            return {
                success: false,
                message: "Error sending verification email: " + error.message
            };
        }
    },

    getCurrentUser: () => {
        const user = auth.currentUser;
        if (user && user.emailVerified) {
            return {
                email: user.email,
                uid: user.uid
            };
        }
        return null;
    },

    isAuthenticated: () => {
        const user = auth.currentUser;
        return user && user.emailVerified;
    }
};

// Firebase Firestore functions
window.firebaseFirestore = {
    addGig: async (gigData) => {
        try {
            const docRef = await addDoc(collection(db, "gigs"), {
                ...gigData,
                createdAt: serverTimestamp()
            });
            return {
                success: true,
                message: "Gig created successfully!",
                id: docRef.id
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to create gig: " + error.message
            };
        }
    },

    getGigs: async () => {
        try {
            const q = query(collection(db, "gigs"), orderBy("createdAt", "desc"));
            const querySnapshot = await getDocs(q);
            const gigs = [];
            
            querySnapshot.forEach((doc) => {
                const data = doc.data();
                gigs.push({
                    id: doc.id,
                    ...data,
                    createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                });
            });
            
            return {
                success: true,
                data: gigs
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to get gigs: " + error.message,
                data: []
            };
        }
    },

    getUserGigs: async (userId) => {
        try {
            // Try the optimized query first
            try {
                const q = query(
                    collection(db, "gigs"), 
                    where("createdBy", "==", userId), 
                    orderBy("createdAt", "desc")
                );
                const querySnapshot = await getDocs(q);
                const gigs = [];
                
                querySnapshot.forEach((doc) => {
                    const data = doc.data();
                    gigs.push({
                        id: doc.id,
                        ...data,
                        createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                    });
                });
                
                return {
                    success: true,
                    data: gigs
                };
            } catch (indexError) {
                // Fallback: Get all gigs and filter client-side
                const allGigsQuery = query(collection(db, "gigs"));
                const querySnapshot = await getDocs(allGigsQuery);
                const gigs = [];
                
                querySnapshot.forEach((doc) => {
                    const data = doc.data();
                    if (data.createdBy === userId) {
                        gigs.push({
                            id: doc.id,
                            ...data,
                            createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                        });
                    }
                });
                
                gigs.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
                
                return {
                    success: true,
                    data: gigs
                };
            }
        } catch (error) {
            return {
                success: false,
                message: "Failed to get user gigs: " + error.message,
                data: []
            };
        }
    },

    getGigById: async (gigId) => {
        try {
            const docRef = doc(db, "gigs", gigId);
            const docSnap = await getDoc(docRef);
            
            if (docSnap.exists()) {
                const data = docSnap.data();
                return {
                    success: true,
                    data: {
                        id: docSnap.id,
                        ...data,
                        createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                    }
                };
            } else {
                return {
                    success: false,
                    message: "Gig not found"
                };
            }
        } catch (error) {
            return {
                success: false,
                message: "Failed to get gig: " + error.message
            };
        }
    },

    updateGig: async (gigId, gigData) => {
        try {
            const docRef = doc(db, "gigs", gigId);
            await updateDoc(docRef, gigData);
            return {
                success: true,
                message: "Gig updated successfully!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to update gig: " + error.message
            };
        }
    },

    deleteGig: async (gigId, userId) => {
        try {
            const docRef = doc(db, "gigs", gigId);
            const docSnap = await getDoc(docRef);
            
            if (!docSnap.exists()) {
                return {
                    success: false,
                    message: "Gig not found or already deleted"
                };
            }
            
            const gigData = docSnap.data();
            if (gigData.createdBy !== userId) {
                return {
                    success: false,
                    message: "You are not authorized to delete this gig"
                };
            }
            
            await deleteDoc(docRef);
            return {
                success: true,
                message: "Gig deleted successfully!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to delete gig: " + error.message
            };
        }
    },

    addStockPhoto: async (photoData) => {
        try {
            const docRef = await addDoc(collection(db, "stockPhotos"), {
                ...photoData,
                createdAt: serverTimestamp(),
                downloads: 0
            });
            return {
                success: true,
                message: "Photo uploaded successfully!",
                id: docRef.id
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to upload photo: " + error.message
            };
        }
    },

    getStockPhotos: async () => {
        try {
            const q = query(collection(db, "stockPhotos"), orderBy("createdAt", "desc"));
            const querySnapshot = await getDocs(q);
            const photos = [];
            
            querySnapshot.forEach((doc) => {
                const data = doc.data();
                photos.push({
                    id: doc.id,
                    ...data,
                    createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                });
            });
            
            return {
                success: true,
                data: photos
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to get photos: " + error.message,
                data: []
            };
        }
    },

    getUserStockPhotos: async (userId) => {
        try {
            try {
                const q = query(
                    collection(db, "stockPhotos"), 
                    where("uploadedBy", "==", userId), 
                    orderBy("createdAt", "desc")
                );
                const querySnapshot = await getDocs(q);
                const photos = [];
                
                querySnapshot.forEach((doc) => {
                    const data = doc.data();
                    photos.push({
                        id: doc.id,
                        ...data,
                        createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                    });
                });
                
                return {
                    success: true,
                    data: photos
                };
            } catch (indexError) {
                const allPhotosQuery = query(collection(db, "stockPhotos"));
                const querySnapshot = await getDocs(allPhotosQuery);
                const photos = [];
                
                querySnapshot.forEach((doc) => {
                    const data = doc.data();
                    if (data.uploadedBy === userId) {
                        photos.push({
                            id: doc.id,
                            ...data,
                            createdAt: data.createdAt?.toDate?.()?.toISOString() || new Date().toISOString()
                        });
                    }
                });
                
                photos.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
                
                return {
                    success: true,
                    data: photos
                };
            }
        } catch (error) {
            return {
                success: false,
                message: "Failed to get user photos: " + error.message,
                data: []
            };
        }
    },

    updateStockPhoto: async (photoId, updateData) => {
        try {
            const docRef = doc(db, "stockPhotos", photoId);
            await updateDoc(docRef, updateData);
            return {
                success: true,
                message: "Photo updated successfully!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to update photo: " + error.message
            };
        }
    },

    deleteStockPhoto: async (photoId) => {
        try {
            const docRef = doc(db, "stockPhotos", photoId);
            await deleteDoc(docRef);
            return {
                success: true,
                message: "Photo deleted successfully!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to delete photo: " + error.message
            };
        }
    },

    incrementDownload: async (photoId) => {
        try {
            const docRef = doc(db, "stockPhotos", photoId);
            const docSnap = await getDoc(docRef);
            
            if (docSnap.exists()) {
                const currentDownloads = docSnap.data().downloads || 0;
                await updateDoc(docRef, {
                    downloads: currentDownloads + 1
                });
            }
            
            return {
                success: true,
                message: "Download counted!"
            };
        } catch (error) {
            return {
                success: false,
                message: "Failed to count download: " + error.message
            };
        }
    }
};
